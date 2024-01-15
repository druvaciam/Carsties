using AuctionService.Data;
using AuctionService.Entities;
using Contracts;
using MassTransit;

namespace AuctionService;

public class AuctionFinishedConsumer(AuctionDbContext dbContext) : IConsumer<AuctionFinished>
{
	public async Task Consume(ConsumeContext<AuctionFinished> context)
	{
		Console.WriteLine("--> consumimng auction finished");
		var auction = await dbContext.Auctions.FindAsync(context.Message.AuctionId);
		if (context.Message.ItemSold)
		{
			auction.Winner = context.Message.Winner;
			auction.SoldAmount = context.Message.Amount;
		}
		
		auction.Status = auction.SoldAmount > auction.ReservePrice
			? Status.Finished : Status.ReserveNotMet;
			
		await dbContext.SaveChangesAsync(); 
	}
}
