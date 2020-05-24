using System;
using System.Device.Gpio;
using System.Diagnostics;
using System.Threading.Tasks;

namespace DistanceSensorTest
{
  class Program
  {
    private static GpioController gpio;
    private static Stopwatch programRuntime;
    private static readonly int triggerPin = 18;
    private static readonly int echoPin = 4;

    static async Task Main(string[] args)
    {
      System.Console.WriteLine("Start...");
      InitiateGpio();

      programRuntime = new Stopwatch();
      programRuntime.Start();

      try
      {
        await MeasureDistance();
      }
      catch (System.Exception ex)
      {
        System.Console.WriteLine(ex);
        throw ex;
      }
      finally
      {
        gpio.Dispose();
        System.Console.WriteLine("GPIO disposed");
      }

    }

    private static async Task MeasureDistance()
    {
      var echoDuration = new Stopwatch();
      var waitingForEcho = new Stopwatch();
      int index = 1;

      while (programRuntime.Elapsed.TotalSeconds <= 40)
      {
        System.Console.WriteLine($"Attempt number: {index}");

        // trigger
        gpio.Write(triggerPin, PinValue.High);
        await Task.Delay(TimeSpan.FromTicks(100));
        gpio.Write(triggerPin, PinValue.Low);

        // Wait for High state on echo pin
        waitingForEcho.Reset();
        waitingForEcho.Start();
        while (gpio.Read(echoPin) == PinValue.Low && waitingForEcho.Elapsed.TotalSeconds <= 1) { }
        waitingForEcho.Stop();
        // Console.WriteLine($"Waiting for High value on Echo pin: {waitingForEcho.Elapsed.Milliseconds}");

        // Measure duration of high state on echo pin
        echoDuration.Reset();
        echoDuration.Start();
        while (gpio.Read(echoPin) == PinValue.High) { }
        echoDuration.Stop();
        var duration = echoDuration.Elapsed.Ticks / 10;
        Console.WriteLine($"High value duration: {duration} microseconds");

        var distance = (duration * 34.0) / 1000 / 2;
        Console.WriteLine($"Measured distance: {distance} {Math.Round(distance)}");

        await Task.Delay(TimeSpan.FromSeconds(1));
        index++;
      }
    }

    static void InitiateGpio()
    {
      gpio = new GpioController();
      gpio.OpenPin(echoPin, PinMode.InputPullDown);
      gpio.OpenPin(triggerPin, PinMode.Output);

      System.Console.WriteLine("GPIO initialized.");
    }
  }
}
