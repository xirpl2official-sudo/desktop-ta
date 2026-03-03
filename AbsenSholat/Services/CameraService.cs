using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using OpenCvSharp;

namespace AbsenSholat.Services
{
    /// <summary>
    /// Camera service using OpenCvSharp + WriteableBitmap for high-performance WPF camera access.
    /// Architecture: Task + CancellationToken + WriteableBitmap.Lock/Unlock
    /// Recommended approach for .NET 6/.NET 8 WPF applications.
    /// </summary>
    public class CameraService : IDisposable
    {
        private VideoCapture? _videoCapture;
        private CancellationTokenSource? _cancellationTokenSource;
        private Task? _captureTask;
        private WriteableBitmap? _writeableBitmap;
        private bool _isRunning = false;
        private bool _isDisposed = false;
        
        private int _frameWidth = 640;
        private int _frameHeight = 480;

        public bool IsRunning => _isRunning;
        public event Action<BitmapSource>? FrameCaptured;
        public event Action<string>? StatusChanged;
        public event Action<Exception>? ErrorOccurred;
        public event Action<string>? QrDetected;

        // Throttling for QR detection
        private DateTime _lastQrCheck = DateTime.MinValue;
        private readonly TimeSpan _qrCheckInterval = TimeSpan.FromMilliseconds(500);

        /// <summary>
        /// Starts the camera capture using OpenCvSharp VideoCapture.
        /// </summary>
        /// <param name="width">Frame width (default: 640)</param>
        /// <param name="height">Frame height (default: 480)</param>
        /// <param name="cameraIndex">Camera device index (default: 0)</param>
        /// <returns>True if camera started successfully, false otherwise</returns>
        public bool StartCamera(int width = 640, int height = 480, int cameraIndex = 0)
        {
            try
            {
                if (_isRunning)
                {
                    Logger.Warning("CameraService", "Camera is already running");
                    return true;
                }

                Logger.Info("CameraService", $"Starting camera (OpenCvSharp Mode) - Index: {cameraIndex}, Resolution: {width}x{height}");
                StatusChanged?.Invoke("Initializing camera...");

                _frameWidth = width;
                _frameHeight = height;

                // Initialize VideoCapture with DirectShow backend for better Windows compatibility
                _videoCapture = new VideoCapture(cameraIndex, VideoCaptureAPIs.DSHOW);
                
                if (!_videoCapture.IsOpened())
                {
                    // Fallback to default API if DirectShow fails
                    Logger.Warning("CameraService", "DirectShow failed, trying default API...");
                    _videoCapture.Dispose();
                    _videoCapture = new VideoCapture(cameraIndex);
                    
                    if (!_videoCapture.IsOpened())
                    {
                        throw new Exception($"Failed to open camera {cameraIndex}");
                    }
                }

                // Set camera properties
                _videoCapture.Set(VideoCaptureProperties.FrameWidth, width);
                _videoCapture.Set(VideoCaptureProperties.FrameHeight, height);
                _videoCapture.Set(VideoCaptureProperties.Fps, 30);

                // Get actual resolution (camera might not support requested resolution)
                int actualWidth = (int)_videoCapture.Get(VideoCaptureProperties.FrameWidth);
                int actualHeight = (int)_videoCapture.Get(VideoCaptureProperties.FrameHeight);
                Logger.Info("CameraService", $"Actual resolution: {actualWidth}x{actualHeight}");
                
                _frameWidth = actualWidth;
                _frameHeight = actualHeight;

                // Initialize WriteableBitmap on UI thread
                Application.Current.Dispatcher.Invoke(() =>
                {
                    _writeableBitmap = new WriteableBitmap(
                        _frameWidth, 
                        _frameHeight, 
                        96, 96, 
                        PixelFormats.Bgr24, 
                        null);
                });

                // Start capture task
                _cancellationTokenSource = new CancellationTokenSource();
                _captureTask = Task.Run(() => CaptureLoop(_cancellationTokenSource.Token), _cancellationTokenSource.Token);

                _isRunning = true;
                Logger.Success("CameraService", "Camera started successfully");
                StatusChanged?.Invoke("Camera started");

                return true;
            }
            catch (Exception ex)
            {
                Logger.Error("CameraService", "Start failed", ex);
                ErrorOccurred?.Invoke(ex);
                StopCamera();
                return false;
            }
        }

        /// <summary>
        /// Overload for backward compatibility (ignores parentHandle parameter)
        /// </summary>
        public bool StartCamera(IntPtr parentHandle, int width = 640, int height = 480, int cameraIndex = 0)
        {
            // parentHandle not needed for OpenCvSharp approach
            return StartCamera(width, height, cameraIndex);
        }

        private void CaptureLoop(CancellationToken cancellationToken)
        {
            using var frame = new Mat();
            
            while (!cancellationToken.IsCancellationRequested && _isRunning)
            {
                try
                {
                    if (_videoCapture == null || !_videoCapture.IsOpened())
                    {
                        Logger.Warning("CameraService", "VideoCapture is not available");
                        break;
                    }

                    // Read frame from camera
                    if (!_videoCapture.Read(frame) || frame.Empty())
                    {
                        Thread.Sleep(10);
                        continue;
                    }

                    // Resize if needed
                    Mat processedFrame = frame;
                    if (frame.Width != _frameWidth || frame.Height != _frameHeight)
                    {
                        processedFrame = new Mat();
                        Cv2.Resize(frame, processedFrame, new OpenCvSharp.Size(_frameWidth, _frameHeight));
                    }

                    // Ensure BGR24 format
                    if (processedFrame.Channels() != 3)
                    {
                        var bgr = new Mat();
                        Cv2.CvtColor(processedFrame, bgr, ColorConversionCodes.GRAY2BGR);
                        if (processedFrame != frame) processedFrame.Dispose();
                        processedFrame = bgr;
                    }

                    // QR Detection (Periodic)
                    if (DateTime.Now - _lastQrCheck > _qrCheckInterval)
                    {
                        _lastQrCheck = DateTime.Now;
                        try
                        {
                            using var detector = new QRCodeDetector();
                            string decodedInfo = detector.DetectAndDecode(processedFrame, out var points);
                            if (!string.IsNullOrEmpty(decodedInfo))
                            {
                                QrDetected?.Invoke(decodedInfo);
                            }
                        }
                        catch (Exception ex)
                        {
                            Logger.Debug("CameraService", $"QR Detection validation failed: {ex.Message}");
                        }
                    }

                    // Flip horizontally (mirror effect)
                    var flippedFrame = new Mat();
                    Cv2.Flip(processedFrame, flippedFrame, FlipMode.Y);
                    if (processedFrame != frame) processedFrame.Dispose();
                    processedFrame = flippedFrame;

                    // Update WriteableBitmap on UI thread
                    Application.Current?.Dispatcher.Invoke(() =>
                    {
                        try
                        {
                            if (_writeableBitmap == null || !_isRunning) return;

                            // Lock the bitmap for writing
                            _writeableBitmap.Lock();
                            try
                            {
                                // Calculate stride
                                int stride = _frameWidth * 3;
                                
                                // Copy pixel data directly to the back buffer
                                unsafe
                                {
                                    byte* pBackBuffer = (byte*)_writeableBitmap.BackBuffer;
                                    byte* pFrame = (byte*)processedFrame.Data;
                                    
                                    // Copy directly without flip
                                    for (int y = 0; y < _frameHeight; y++)
                                    {
                                        Buffer.MemoryCopy(
                                            pFrame + (y * stride),
                                            pBackBuffer + (y * _writeableBitmap.BackBufferStride),
                                            stride,
                                            stride);
                                    }
                                }
                                
                                // Specify the area to update
                                _writeableBitmap.AddDirtyRect(new Int32Rect(0, 0, _frameWidth, _frameHeight));
                            }
                            finally
                            {
                                _writeableBitmap.Unlock();
                            }

                            // Notify listeners with the frozen bitmap
                            var frozenBitmap = _writeableBitmap.Clone();
                            frozenBitmap.Freeze();
                            FrameCaptured?.Invoke(frozenBitmap);
                        }
                        catch (Exception ex)
                        {
                            // Ignore frame update errors to prevent capture loop interruption
                            Logger.Debug("CameraService", $"Frame update error: {ex.Message}");
                        }
                    }, System.Windows.Threading.DispatcherPriority.Render);

                    // Clean up resized frame if created
                    if (processedFrame != frame)
                    {
                        processedFrame.Dispose();
                    }

                    // Target ~30 FPS
                    Thread.Sleep(33);
                }
                catch (OperationCanceledException)
                {
                    // Expected when stopping
                    break;
                }
                catch (Exception ex)
                {
                    Logger.Error("CameraService", "Capture loop error", ex);
                    Thread.Sleep(100); // Prevent tight error loop
                }
            }

            Logger.Info("CameraService", "Capture loop ended");
        }

        /// <summary>
        /// Stops the camera capture and releases resources.
        /// </summary>
        public void StopCamera()
        {
            try
            {
                Logger.Info("CameraService", "Stopping camera...");
                
                _isRunning = false;
                
                // Cancel the capture task
                if (_cancellationTokenSource != null)
                {
                    _cancellationTokenSource.Cancel();
                    
                    // Wait for capture task to complete
                    try
                    {
                        _captureTask?.Wait(TimeSpan.FromSeconds(2));
                    }
                    catch (AggregateException)
                    {
                        // Task was cancelled, expected
                    }
                    
                    _cancellationTokenSource.Dispose();
                    _cancellationTokenSource = null;
                    _captureTask = null;
                }

                // Release VideoCapture
                if (_videoCapture != null)
                {
                    _videoCapture.Release();
                    _videoCapture.Dispose();
                    _videoCapture = null;
                }

                _writeableBitmap = null;

                Logger.Info("CameraService", "Camera stopped");
                StatusChanged?.Invoke("Camera stopped");
            }
            catch (Exception ex)
            {
                Logger.Error("CameraService", "Stop error", ex);
            }
        }

        /// <summary>
        /// Gets the count of available camera devices.
        /// </summary>
        /// <returns>Number of available cameras</returns>
        public static int GetAvailableCameraCount()
        {
            int count = 0;
            
            // Try up to 5 camera indices
            for (int i = 0; i < 5; i++)
            {
                try
                {
                    using var capture = new VideoCapture(i, VideoCaptureAPIs.DSHOW);
                    if (capture.IsOpened())
                    {
                        count++;
                        capture.Release();
                    }
                    else
                    {
                        break; // Stop at first unavailable camera
                    }
                }
                catch
                {
                    break;
                }
            }
            
            return count;
        }

        /// <summary>
        /// Gets the current frame as a BitmapSource (for single capture use cases).
        /// </summary>
        /// <returns>Current frame as BitmapSource, or null if not available</returns>
        public BitmapSource? GetCurrentFrame()
        {
            if (_writeableBitmap == null) return null;
            
            BitmapSource? result = null;
            Application.Current?.Dispatcher.Invoke(() =>
            {
                if (_writeableBitmap != null)
                {
                    result = _writeableBitmap.Clone();
                    result.Freeze();
                }
            });
            
            return result;
        }

        public void Dispose()
        {
            if (!_isDisposed)
            {
                StopCamera();
                _isDisposed = true;
            }
        }
    }
}
