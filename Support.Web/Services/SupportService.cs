using Microsoft.EntityFrameworkCore;
using Support.Data;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Support.Services;

public class SupportService
{
    private readonly SupportContext _context;

    public SupportService(SupportContext context)
    {
        _context = context;
    }

    // Retrieval Methods for Querying
    public async Task<List<SupportArea>> GetSupportAreasAsync()
    {
        return await _context.SupportAreas.ToListAsync();
    }

    public async Task<List<SpecificIssue>> GetIssuesByAreaAsync(int supportAreaId)
    {
        return await _context.SpecificIssues
            .Where(i => i.SupportAreaId == supportAreaId)
            .ToListAsync();
    }

    public async Task<SpecificIssue?> GetIssueByIdAsync(int issueId)
    {
        return await _context.SpecificIssues
            .Include(i => i.Solutions)
            .FirstOrDefaultAsync(i => i.Id == issueId);
    }

    public async Task<List<Solution>> GetSolutionsByIssueAsync(int issueId)
    {
        var solutions = await _context.Solutions
            .Where(s => s.SpecificIssueId == issueId)
            .ToListAsync();

        foreach (var solution in solutions)
        {
            if (solution.Request != null)
            {
                var request = await _context.PlayWrightRequests
                    .Include(r => r.WebTasks)
                    .FirstOrDefaultAsync(r => r.Id == solution.Request);

                if (request != null)
                {
                    try
                    {
                        var json = JsonSerializer.Serialize(request, new JsonSerializerOptions 
                        { 
                            ReferenceHandler = ReferenceHandler.IgnoreCycles
                        });
                        using var client = new TcpClient();
                        await client.ConnectAsync("127.0.0.1", 13000);
                        using var stream = client.GetStream();
                        var data = Encoding.UTF8.GetBytes(json + "\r\n");
                        await stream.WriteAsync(data, 0, data.Length);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error sending TCP data: {ex.Message}");
                    }
                }
            }
        }

        return solutions;
    }

    // Admin CRUD Methods - Support Areas
    public async Task AddSupportAreaAsync(SupportArea area)
    {
        _context.SupportAreas.Add(area);
        await _context.SaveChangesAsync();
    }
    public async Task UpdateSupportAreaAsync(SupportArea area)
    {
        _context.Entry(area).State = EntityState.Modified;
        await _context.SaveChangesAsync();
    }
    public async Task DeleteSupportAreaAsync(int id)
    {
        var area = await _context.SupportAreas.FindAsync(id);
        if (area != null)
        {
            _context.SupportAreas.Remove(area);
            await _context.SaveChangesAsync();
        }
    }

    // Admin CRUD Methods - Specific Issues
    public async Task AddSpecificIssueAsync(SpecificIssue issue)
    {
        _context.SpecificIssues.Add(issue);
        await _context.SaveChangesAsync();
    }
    public async Task UpdateSpecificIssueAsync(SpecificIssue issue)
    {
        _context.Entry(issue).State = EntityState.Modified;
        await _context.SaveChangesAsync();
    }
    public async Task DeleteSpecificIssueAsync(int id)
    {
        var issue = await _context.SpecificIssues.FindAsync(id);
        if (issue != null)
        {
            _context.SpecificIssues.Remove(issue);
            await _context.SaveChangesAsync();
        }
    }

    // Admin CRUD Methods - Solutions
    public async Task AddSolutionAsync(Solution solution)
    {
        _context.Solutions.Add(solution);
        await _context.SaveChangesAsync();
    }
    public async Task UpdateSolutionAsync(Solution solution)
    {
        _context.Entry(solution).State = EntityState.Modified;
        await _context.SaveChangesAsync();
    }
    public async Task DeleteSolutionAsync(int id)
    {
        var solution = await _context.Solutions.FindAsync(id);
        if (solution != null)
        {
            _context.Solutions.Remove(solution);
            await _context.SaveChangesAsync();
        }
    }
}
