using Microsoft.EntityFrameworkCore;
using PensionVault.Application.DTOs.Employers;
using PensionVault.Domain.Entities;
using PensionVault.Domain.Enums;

namespace PensionVault.Application.Services;

public interface IEmployerService
{
    Task<IEnumerable<EmployerResponse>> GetAllAsync();
    Task<EmployerResponse> GetByIdAsync(Guid id);
    Task<EmployerResponse> CreateAsync(CreateEmployerRequest request);
    Task<EmployerResponse> UpdateAsync(Guid id, UpdateEmployerRequest request);
}

public class EmployerService : IEmployerService
{
    private readonly IAppDbContext _context;
    public EmployerService(IAppDbContext context) => _context = context;

    public async Task<IEnumerable<EmployerResponse>> GetAllAsync() =>
        await _context.Employers.Select(e => ToResponse(e)).ToListAsync();

    public async Task<EmployerResponse> GetByIdAsync(Guid id)
    {
        var e = await _context.Employers.FindAsync(id)
            ?? throw new KeyNotFoundException("Employer not found.");
        return ToResponse(e);
    }

    public async Task<EmployerResponse> CreateAsync(CreateEmployerRequest request)
    {
        if (await _context.Employers.AnyAsync(e => e.RegistrationNumber == request.RegistrationNumber))
            throw new InvalidOperationException("Registration number already exists.");

        var employer = new Employer
        {
            CompanyName = request.CompanyName,
            RegistrationNumber = request.RegistrationNumber,
            Industry = request.Industry,
            RemittanceFrequency = request.RemittanceFrequency,
            ContactDetails = request.ContactDetails,
            Status = EmployerStatus.Active
        };
        _context.Employers.Add(employer);
        await _context.SaveChangesAsync();
        return ToResponse(employer);
    }

    public async Task<EmployerResponse> UpdateAsync(Guid id, UpdateEmployerRequest request)
    {
        var employer = await _context.Employers.FindAsync(id)
            ?? throw new KeyNotFoundException("Employer not found.");
        employer.CompanyName = request.CompanyName;
        employer.Industry = request.Industry;
        employer.RemittanceFrequency = request.RemittanceFrequency;
        employer.ContactDetails = request.ContactDetails;
        employer.Status = request.Status;
        await _context.SaveChangesAsync();
        return ToResponse(employer);
    }

    private static EmployerResponse ToResponse(Employer e) => new(
        e.EmployerId, e.CompanyName, e.RegistrationNumber, e.Industry,
        e.EnrolledMemberCount, e.RemittanceFrequency, e.ContactDetails, e.Status);
}
