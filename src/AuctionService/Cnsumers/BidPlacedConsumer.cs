using AuctionService.Data;
using Contracts;
using MassTransit;

namespace AuctionService;

public class BidPlacedConsumer(AuctionDbContext dbContext) : IConsumer<BidPlaced>
{
	public async Task Consume(ConsumeContext<BidPlaced> context)
	{
		Console.WriteLine("--> consumimng bid placed");
		var auction = await dbContext.Auctions.FindAsync(context.Message.AuctionId);
	   	if (auction.CurrentHighBid == null
	   		|| context.Message.BidStatus.Contains("accepted", StringComparison.InvariantCultureIgnoreCase)
			&& context.Message.Amount > auction.CurrentHighBid)
		{
			auction.CurrentHighBid = context.Message.Amount;
			await dbContext.SaveChangesAsync();
		}
	}
}
