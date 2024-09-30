using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using SecureFlight.Api.Models;
using SecureFlight.Api.Utils;
using SecureFlight.Core.Entities;
using SecureFlight.Core.Interfaces;

namespace SecureFlight.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class PassengersController(
    IService<Passenger> personService,
    IService<Flight> flightService,
    IRepository<Flight> flightRepository,
    IRepository<Person> personRepository,
    IMapper mapper)
    : SecureFlightBaseController(mapper)
{
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<PassengerDataTransferObject>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ErrorResponseActionResult))]
    public async Task<IActionResult> Get()
    {
        var passengers = await personService.GetAllAsync();
        return MapResultToDataTransferObject<IReadOnlyList<Passenger>, IReadOnlyList<PassengerDataTransferObject>>(passengers);
    }
    
    [HttpGet("/flights/{flightId:long}/passengers")]
    [ProducesResponseType(typeof(IEnumerable<PassengerDataTransferObject>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ErrorResponseActionResult))]
    public async Task<IActionResult> GetPassengersByFlight(long flightId)
    {
        var passengers = await personService.FilterAsync(p => p.Flights.Any(x => x.Id == flightId));
        return !passengers.Succeeded ?
            NotFound($"No passengers were found for the flight {flightId}") :
            MapResultToDataTransferObject<IReadOnlyList<Passenger>, IReadOnlyList<PassengerDataTransferObject>>(passengers);
    }

    [HttpPost("/flights/{flightId:long}/{personId}")]
    [ProducesResponseType(typeof(IEnumerable<FlightDataTransferObject>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ErrorResponseActionResult))]
    public async Task<IActionResult> AddPassenger(long flightId, string personId)
    {
        var flight = await flightService.FindAsync(flightId);
        if(flight != null)
        {
            var passengerToAdd = await personService.FindAsync(personId);
            if (passengerToAdd != null)
            {
                flight.Result.Passengers.Add(passengerToAdd);
                flightRepository.Update(flight);
                flightRepository.SaveChangesAsync();    
                return Ok();
            }
        }
        return BadRequest("The person or the flight doesn't exist");
    }

    [HttpDelete("/flights/{flightId:long}/{personId}")]
    [ProducesResponseType(typeof(IEnumerable<FlightDataTransferObject>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ErrorResponseActionResult))]
    public async Task<IActionResult> RemovePassenger(long flightId, string personId)
    {
        var flight = await flightService.FindAsync(flightId);
        if (flight != null)
        {
            var passengerToAdd = await personService.FindAsync(personId);
            if (passengerToAdd != null)
            {
                flight.Result.Passengers.Remove(passengerToAdd);
                flightRepository.Update(flight);
                flightRepository.SaveChangesAsync();
                //return Ok();
            }
            
            var passengerInOtherFlights = await personService.FilterAsync(p => p.Flights.Any(x => x.Passengers.Any(ps => ps.Id == personId)));
            if(passengerInOtherFlights.Result.Count > 0)
            {
                var personToRemove = personService.FindAsync(personId);
                person
                personRepository.GetByIdAsync(personId);
                    personRepository.
            }

        }
        return BadRequest("The person or the flight doesn't exist");
    }
}