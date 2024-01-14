using System;
using System.Collections.Generic;
using AutoMapper;
using Contracts;
using MassTransit;
using MongoDB.Entities;
using SearchService.Models;

namespace SearchService.Consumers;

public class AuctionUpdatedConsumer(IMapper mapper) : IConsumer<AuctionUpdated>
{
	public async Task Consume(ConsumeContext<AuctionUpdated> context)
	{
		Console.WriteLine($"--> Consuming auction updated: {context.Message.Id}");
		
		var query = DB.Find<Item>().MatchID(context.Message.Id);
		var result = await query.ExecuteAsync();
		if (result.Count != 1) throw new ArgumentException($"Found {result.Count} items with id {context.Message.Id}.");
		
		var item = mapper.Map(context.Message, result.First());
		item.UpdatedAt = DateTime.UtcNow;
		Console.WriteLine($"--> updated: {item.Model}, {item.UpdatedAt}, {item.AuctionEnd}");
		
		await item.SaveAsync();
	}
}