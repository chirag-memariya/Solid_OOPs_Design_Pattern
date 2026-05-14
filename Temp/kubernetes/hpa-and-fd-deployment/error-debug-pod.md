sharanphadke@400451-4:~$ kubectl get pods
NAME                          READY   STATUS             RESTARTS      AGE
fd-service-55f6bfcf69-p7pqd   0/1     CrashLoopBackOff   7 (17s ago)   17h
load-generator                1/1     Running            0             6m55s

# logs:
sharanphadke@400451-4:~$ kubectl logs fd-service-55f6bfcf69-p7pqd -n kube-system
Error from server (NotFound): pods "fd-service-55f6bfcf69-p7pqd" not found
sharanphadke@400451-4:~$ kubectl logs fd-service-55f6bfcf69-p7pqd 
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: http://[::]:80
info: Microsoft.Hosting.Lifetime[0]
      Application started. Press Ctrl+C to shut down.
info: Microsoft.Hosting.Lifetime[0]
      Hosting environment: Production
info: Microsoft.Hosting.Lifetime[0]
      Content root path: /app/
warn: Microsoft.AspNetCore.HttpsPolicy.HttpsRedirectionMiddleware[3]
      Failed to determine the https port for redirect.
Unhandled exception. System.TypeInitializationException: The type initializer for 'OpenCvSharp.Internal.NativeMethods' threw an exception.
 ---> System.DllNotFoundException: Unable to load shared library 'OpenCvSharpExtern' or one of its dependencies. In order to help diagnose loading problems, consider setting the LD_DEBUG environment variable: libOpenCvSharpExtern: cannot open shared object file: No such file or directory
   at OpenCvSharp.Internal.NativeMethods.redirectError(CvErrorCallback errCallback, IntPtr userdata, IntPtr& prevUserdata)
   at OpenCvSharp.Internal.ExceptionHandler.RegisterExceptionCallback()
   at OpenCvSharp.Internal.NativeMethods.LoadLibraries(IEnumerable`1 additionalPaths)
   at OpenCvSharp.Internal.NativeMethods..cctor()
   --- End of inner exception stack trace ---
   at OpenCvSharp.Internal.NativeMethods.videoio_VideoCapture_new2(String filename, Int32 apiPreference, IntPtr& returnValue)
   at OpenCvSharp.VideoCapture..ctor(String fileName, VideoCaptureAPIs apiPreference)
   at SwarmDemoService.Services.FaceDetectionService.DetectFace(String cameraId, CancellationToken cancellationToken)+MoveNext() in /src/Services/FaceDetectionService.cs:line 10
   at SwarmDemoService.Controllers.FaceDetectionController.<>c__DisplayClass3_0.<<StartFaceDetection>b__0>d.MoveNext() in /src/Controllers/FaceDetectionController.cs:line 39
--- End of stack trace from previous location ---
   at System.Threading.Tasks.Task.<>c.<ThrowAsync>b__128_1(Object state)
   at System.Threading.QueueUserWorkItemCallback.<>c.<.cctor>b__6_0(QueueUserWorkItemCallback quwi)
   at System.Threading.ExecutionContext.RunForThreadPoolUnsafe[TState](ExecutionContext executionContext, Action`1 callback, TState& state)
   at System.Threading.QueueUserWorkItemCallback.Execute()
   at System.Threading.ThreadPoolWorkQueue.Dispatch()
   at System.Threading.PortableThreadPool.WorkerThread.WorkerThreadStart()
   at System.Threading.Thread.StartCallback()


# sharan-phadke image
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: http://[::]:80
info: Microsoft.Hosting.Lifetime[0]
      Application started. Press Ctrl+C to shut down.
info: Microsoft.Hosting.Lifetime[0]
      Hosting environment: Production
info: Microsoft.Hosting.Lifetime[0]
      Content root path: /app/
warn: Microsoft.AspNetCore.HttpsPolicy.HttpsRedirectionMiddleware[3]
      Failed to determine the https port for redirect.
[rtsp @ 0x7f1528089680] method DESCRIBE failed: 401 Unauthorized
Unhandled exception. System.Exception: Failed to open the RTSP stream for Camera( ID=22
   at SwarmDemoService.Services.FaceDetectionService.DetectFace(String cameraId, CancellationToken cancellationToken)+MoveNext() in /home/matrix/Documents/Orchestration/DockerSwarm/SwarmDemoService/Services/FaceDetectionService.cs:line 12
   at SwarmDemoService.Controllers.FaceDetectionController.<>c__DisplayClass3_0.<<StartFaceDetection>b__0>d.MoveNext() in /home/matrix/Documents/Orchestration/DockerSwarm/SwarmDemoService/Controllers/FaceDetectionController.cs:line 39
--- End of stack trace from previous location ---
   at System.Threading.Tasks.Task.<>c.<ThrowAsync>b__128_1(Object state)
   at System.Threading.QueueUserWorkItemCallback.<>c.<.cctor>b__6_0(QueueUserWorkItemCallback quwi)
   at System.Threading.ExecutionContext.RunForThreadPoolUnsafe[TState](ExecutionContext executionContext, Action`1 callback, TState& state)
   at System.Threading.QueueUserWorkItemCallback.Execute()
   at System.Threading.ThreadPoolWorkQueue.Dispatch()
   at System.Threading.PortableThreadPool.WorkerThread.WorkerThreadStart()
   at System.Threading.Thread.StartCallback()
