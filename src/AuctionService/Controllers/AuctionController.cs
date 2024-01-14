using AuctionService.Data;
using AuctionService.DTOs;
using AuctionService.Entities;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Contracts;
using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AuctionService.Controllers;

[ApiController]
[Route("api/auctions")]
public class AuctionController(AuctionDbContext context, IMapper mapper, IPublishEndpoint publishEndpoint) : ControllerBase
{
	[HttpGet]
	public async Task<ActionResult<List<AuctionDto>>> GetAllAuctions(string date)
	{
		var query = context.Auctions.OrderBy(x => x.Item.Make).AsQueryable();
		
		if (!string.IsNullOrEmpty(date))
			query = query.Where(x => x.UpdatedAt.CompareTo(DateTime.Parse(date).ToUniversalTime().AddMilliseconds(1)) > 0);
			
		return await query.ProjectTo<AuctionDto>(mapper.ConfigurationProvider).ToListAsync();
		
		// var auctions = await context.Auctions
		// 	.Include(x => x.Item)
		// 	.OrderBy(x => x.Item.Make)
		// 	.ToListAsync();
		
		// return mapper.Map<List<AuctionDto>>(auctions);
	}
	
	[HttpGet("{id}")]
	public async Task<ActionResult<AuctionDto>> GetAuctionById(Guid id)
	{
		var auction = await context.Auctions
			.Include(x => x.Item)
			.FirstOrDefaultAsync(x => x.Id == id);
		
		return auction is null ? NotFound() : mapper.Map<AuctionDto>(auction);
	}
	
	[HttpPost]
	public async Task<ActionResult<AuctionDto>> CreateAuction(CreateAuctionDto auctionDto)
	{
		var auction = mapper.Map<Auction>(auctionDto);
		// TODO: add current user as seller
		auction.Seller = "test-seller";
		
		await context.Auctions.AddAsync(auction);
		
		var newAuction = mapper.Map<AuctionDto>(auction);
		await publishEndpoint.Publish(mapper.Map<AuctionCreated>(newAuction));
		Console.WriteLine($"--> new auction {newAuction.Id} published to rabbitMQ");
		
		var res = await context.SaveChangesAsync() > 0;
		if (!res)
			return BadRequest("Could not save changes to the db");
			
		return CreatedAtAction(nameof(GetAuctionById), new {auction.Id}, newAuction);
	}
	
	[HttpPut("{id}")]
	public async Task<ActionResult> UpdateAuction(Guid id, UpdateAuctionDto updateAuctionDto)
	{
		var auction = await context.Auctions.Include(x => x.Item).FirstOrDefaultAsync(x => x.Id == id);
		
		if (auction is null)
			return NotFound();
			
		// TODO: check seller == username
		auction.Item.Make = updateAuctionDto.Make ?? auction.Item.Make;
		auction.Item.Model = updateAuctionDto.Model ?? auction.Item.Model;
		auction.Item.Color = updateAuctionDto.Color ?? auction.Item.Color;
		auction.Item.Mileage = updateAuctionDto.Mileage ?? auction.Item.Mileage;
		auction.Item.Year = updateAuctionDto.Year ?? auction.Item.Year;
		
		var updatedAuction = mapper.Map<AuctionDto>(auction);
		await publishEndpoint.Publish(mapper.Map<AuctionUpdated>(updatedAuction));
		Console.WriteLine($"--> updated auction {updatedAuction.Id} published to rabbitMQ");
		
		var res = await context.SaveChangesAsync() > 0;
		
		return res ? Ok() : BadRequest("Failed to save item changes");
	}
	
	[HttpDelete("{id}")]
	public async Task<ActionResult> DeleteAuction(Guid id)
	{
		var auction = await context.Auctions.FindAsync(id);
		
		if (auction is null)
			return NotFound();
			
		// TODO: check seller == username
		
		context.Auctions.Remove(auction);
		
		await publishEndpoint.Publish(new AuctionDeleted{ Id = id.ToString() });
		Console.WriteLine($"--> deleted auction {id} published to rabbitMQ");
		
		var res = await context.SaveChangesAsync() > 0;
		
		return res ? Ok() : BadRequest("Failed to update db");
	}
}