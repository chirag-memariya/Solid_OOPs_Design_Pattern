using System;
using DesignPatternsSingleton.UploadService;

Thread t1= new Thread(() =>
    {
        var instance = UploadService.Instance(1); // id - 1 only
    });

Thread t2= new Thread(() =>
    {
        var instance = UploadService.Instance(2); // id - 1 only, bcz 1st created so 2nd does not create that
    });


t1.Start();
t2.Start();

t1.Join();
t2.Join();