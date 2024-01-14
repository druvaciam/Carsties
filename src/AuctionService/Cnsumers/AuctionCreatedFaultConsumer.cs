using Contracts;
using MassTransit;

namespace AuctionService.Cnsumers;

public class AuctionCreatedFaultConsumer : IConsumer<Fault<AuctionCreated>>
{
	public Task Consume(ConsumeContext<Fault<AuctionCreated>> context)
	{
		Console.WriteLine("--> Consuming faulty creation");
		
		var exception = context.Message.Exceptions.First();
		if (exception.ExceptionType == "System.ArgumentException")
		{
			Console.WriteLine($"Argument Exception excpetion{exception.Message}");
			//context.Message.Message.Model = "FooBar";
			//await context.Publish(context.Message.Message);
		}
		else
		{
			Console.WriteLine($"Exception type: {exception.ExceptionType}, message: {exception.Message}");
		}
		return Task.CompletedTask;
	}
	
	public Task Consume(ConsumeContext<Fault<AuctionUpdated>> context)
	{
		Console.WriteLine("--> Consuming faulty update");
		var exception = context.Message.Exceptions.First();
		Console.WriteLine($"Exception type: {exception.ExceptionType}, message: {exception.Message}");
		return Task.CompletedTask;
	}
}