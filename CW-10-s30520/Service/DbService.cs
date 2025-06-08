using CW_10_s30520.Data;
using CW_10_s30520.Exceptions;
using CW_10_s30520.Models;
using CW_10_s30520.Models.DTOs;
using Microsoft.EntityFrameworkCore;

namespace CW_10_s30520.Service;

public interface IDbService
{
    Task<TripResponseDto> GetTripsAsync(int page, int pageSize);
    Task DeleteClientByIdAsync(int clientId);
    Task AssignClientToTripAsync(ClientTripGetDto dto);
}

public class DbService(Cw10Context data) : IDbService
{
    public async Task<TripResponseDto> GetTripsAsync(int page, int pageSize)
    {
        if (page < 1)
        {
            page = 1;
        }

        if (pageSize < 1)
        {
            pageSize = 10;
        }

        var tripsDataQuery = data.Trips
            .Include(t => t.ClientTrips)
            .ThenInclude(ct => ct.IdClientNavigation)
            .Include(t => t.IdCountries)
            .OrderByDescending(t => t.DateFrom);

        int totalCount = await tripsDataQuery.CountAsync();
        int allPages = (int)Math.Ceiling(totalCount / (double) pageSize);

        var tripsData = await tripsDataQuery
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var result = new TripResponseDto()
        {
            PageNum = page,
            PageSize = pageSize,
            AllPages = allPages,
            Trips = tripsData.Select(t => new TripGetDto
            {
                Name = t.Name,
                Description = t.Description,
                DateFrom = t.DateFrom,
                DateTo = t.DateTo,
                MaxPeople = t.MaxPeople,
                Countries = t.IdCountries.Select(c => new CountryGetDto
                {
                    Name = c.Name
                }).ToList(),
                Clients = t.ClientTrips.Select(ct => new ClientGetDto
                {
                    FirstName = ct.IdClientNavigation.FirstName,
                    LastName = ct.IdClientNavigation.LastName,
                }).ToList()
            }).ToList()
        };
        
        return result;
    }

    public async Task DeleteClientByIdAsync(int clientId)
    {
        var client = await data.Clients
                               .Include(c => c.ClientTrips)
                               .FirstOrDefaultAsync(c => c.IdClient == clientId);

        if (client == null)
        {
            throw new NotFoundException($"Client with id {clientId} not found");
        }

        if (client.ClientTrips.Any())
        {
            throw new Exception("Cannot delete a client who is assigned to at least one trip");
        }

        data.Clients.Remove(client);
        
        await data.SaveChangesAsync();
    }

    public async Task AssignClientToTripAsync(ClientTripGetDto clientTripGetDto)
    {
        var client = await data.Clients.FirstOrDefaultAsync(c => c.Pesel == clientTripGetDto.Pesel);

        if (client != null)
        {
            throw new Exception($"Client with pesel {clientTripGetDto.Pesel} already exists");
        }

        var trip = await data.Trips.FirstOrDefaultAsync(t => t.IdTrip == clientTripGetDto.IdTrip);

        if (trip == null)
        {
            throw new NotFoundException($"Trip with id {clientTripGetDto.IdTrip} not found");
        }

        if (trip.DateFrom <= DateTime.Now)
        {
            throw new Exception("Registering for a trip that already started is not allowed");
        }
        
        using var transaction = await data.Database.BeginTransactionAsync();

        try
        {
            var createdClient = new Client
            {
                FirstName = clientTripGetDto.FirstName,
                LastName = clientTripGetDto.LastName,
                Email = clientTripGetDto.Email,
                Telephone = clientTripGetDto.Telephone,
                Pesel = clientTripGetDto.Pesel,
            };

            data.Clients.Add(createdClient);

            await data.SaveChangesAsync();

            var clientTrip = new ClientTrip
            {
                IdClient = createdClient.IdClient,
                IdTrip = clientTripGetDto.IdTrip,
                RegisteredAt = DateTime.Now,
                PaymentDate = clientTripGetDto.PaymentDate,
            };

            data.ClientTrips.Add(clientTrip);

            await data.SaveChangesAsync();

            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
}