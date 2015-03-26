Greeter.Logger = Require<Logger>();

[Export(typeof(IGreeter))]
public class Greeter : IGreeter {
  internal static Logger Logger {get;set;}
  
  public void Greet(string message) {
    Logger.Info(message);
    Console.WriteLine(message);
  }
}