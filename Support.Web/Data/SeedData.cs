using Microsoft.EntityFrameworkCore;

namespace Support.Data;

public static class SeedData
{
    public static void Initialize(IServiceProvider serviceProvider)
    {
        using var context = new SupportContext(
            serviceProvider.GetRequiredService<DbContextOptions<SupportContext>>());

        if (!context.SupportAreas.Any())
        {

        var supportAreas = new List<SupportArea>
        {
            new SupportArea { Name = "Microsoft Windows", Description = "Operating system related issues, updates, and configuration." },
            new SupportArea { Name = "Hardware", Description = "Physical device issues, peripherals, and components." },
            new SupportArea { Name = "Co Pilot", Description = "AI assistant features, integration, and usage." }
        };

        context.SupportAreas.AddRange(supportAreas);
        context.SaveChanges();

        // Seed Specific Issues for Microsoft Windows
        var windowsArea = context.SupportAreas.First(a => a.Name == "Microsoft Windows");
        var windowsIssues = new List<SpecificIssue>
        {
            new SpecificIssue { SupportAreaId = windowsArea.Id, Name = "Blue Screen of Death (BSOD)", Description = "System crashes with a blue screen error code." },
            new SpecificIssue { SupportAreaId = windowsArea.Id, Name = "Windows Update Failed", Description = "Updates fail to download or install." },
            new SpecificIssue { SupportAreaId = windowsArea.Id, Name = "Slow Performance", Description = "System is running sluggishly or freezing." },
            new SpecificIssue { SupportAreaId = windowsArea.Id, Name = "Network Connection Lost", Description = "Cannot connect to the internet or local network." },
            new SpecificIssue { SupportAreaId = windowsArea.Id, Name = "Audio Not Working", Description = "No sound output from speakers or headphones." },
            new SpecificIssue { SupportAreaId = windowsArea.Id, Name = "Printer Not Detected", Description = "Windows cannot find the connected printer." },
            new SpecificIssue { SupportAreaId = windowsArea.Id, Name = "Start Menu Not Opening", Description = "Clicking the Start button does nothing." },
            new SpecificIssue { SupportAreaId = windowsArea.Id, Name = "File Explorer Crashing", Description = "File Explorer closes unexpectedly." },
            new SpecificIssue { SupportAreaId = windowsArea.Id, Name = "Application Not Responding", Description = "Programs freeze and must be force closed." },
            new SpecificIssue { SupportAreaId = windowsArea.Id, Name = "Login Issues", Description = "Cannot sign in to user account." }
        };
        context.SpecificIssues.AddRange(windowsIssues);
        context.SaveChanges();

        // Seed Specific Issues for Hardware
        var hardwareArea = context.SupportAreas.First(a => a.Name == "Hardware");
        var hardwareIssues = new List<SpecificIssue>
        {
            new SpecificIssue { SupportAreaId = hardwareArea.Id, Name = "Monitor No Signal", Description = "Display is black and shows 'No Signal'." },
            new SpecificIssue { SupportAreaId = hardwareArea.Id, Name = "Keyboard Malfunction", Description = "Keys are not registering or typing incorrect characters." },
            new SpecificIssue { SupportAreaId = hardwareArea.Id, Name = "Mouse Not Moving", Description = "Cursor is stuck or tracking poorly." },
            new SpecificIssue { SupportAreaId = hardwareArea.Id, Name = "Overheating", Description = "Computer feels hot and fan is loud." },
            new SpecificIssue { SupportAreaId = hardwareArea.Id, Name = "Hard Drive Noise", Description = "Clicking or grinding sounds from storage." },
            new SpecificIssue { SupportAreaId = hardwareArea.Id, Name = "USB Device Not Recognized", Description = "Plugged in device is not showing up." },
            new SpecificIssue { SupportAreaId = hardwareArea.Id, Name = "Laptop Battery Not Charging", Description = "Plugged in but battery percentage not increasing." },
            new SpecificIssue { SupportAreaId = hardwareArea.Id, Name = "Webcam Black Screen", Description = "Camera light is on but image is black." },
            new SpecificIssue { SupportAreaId = hardwareArea.Id, Name = "Microphone Too Quiet", Description = "Others cannot hear voice clearly." },
            new SpecificIssue { SupportAreaId = hardwareArea.Id, Name = "Bluetooth Pairing Failed", Description = "Cannot connect wireless device." }
        };
        context.SpecificIssues.AddRange(hardwareIssues);
        context.SaveChanges();

        // Seed Specific Issues for Co Pilot
        var copilotArea = context.SupportAreas.First(a => a.Name == "Co Pilot");
        var copilotIssues = new List<SpecificIssue>
        {
            new SpecificIssue { SupportAreaId = copilotArea.Id, Name = "Response Hallucination", Description = "AI provides factually incorrect information." },
            new SpecificIssue { SupportAreaId = copilotArea.Id, Name = "Context Limit Reached", Description = "Conversation is too long to continue." },
            new SpecificIssue { SupportAreaId = copilotArea.Id, Name = "Code Generation Error", Description = "Generated code does not compile or run." },
            new SpecificIssue { SupportAreaId = copilotArea.Id, Name = "Slow Response Time", Description = "AI takes a long time to generate a reply." },
            new SpecificIssue { SupportAreaId = copilotArea.Id, Name = "Safety Filter Triggered", Description = "Request blocked by content policy." },
            new SpecificIssue { SupportAreaId = copilotArea.Id, Name = "Integration Missing", Description = "Co Pilot not appearing in Office apps." },
            new SpecificIssue { SupportAreaId = copilotArea.Id, Name = "Voice Mode Not Working", Description = "Speech input is not recognized." },
            new SpecificIssue { SupportAreaId = copilotArea.Id, Name = "Image Generation Failed", Description = "DALL-E request returns an error." },
            new SpecificIssue { SupportAreaId = copilotArea.Id, Name = "History Not Saved", Description = "Previous chats are missing." },
            new SpecificIssue { SupportAreaId = copilotArea.Id, Name = "Login Loop", Description = "Authentication keeps asking for credentials." }
        };
        context.SpecificIssues.AddRange(copilotIssues);
        context.SaveChanges();


        // Seed Solutions (One for each issue)
        var issues = context.SpecificIssues.ToList();
        foreach (var issue in issues)
        {
            context.Solutions.Add(new Solution
            {
                SpecificIssueId = issue.Id,
                Name = $"Fix for {issue.Name}",
                Description = $"This is a verified solution for {issue.Name}. Please follow the standard troubleshooting steps: 1. Restart the related service/device. 2. Check for updates. 3. Consult the official documentation for error code 0x..."
            });
        }
        context.SaveChanges();
    }


        // Seed Nimbus System and related data
        if (!context.SupportAreas.Any(a => a.Name == "Nimbus System"))
        {
            var nimbusArea = new SupportArea 
            { 
                Name = "Nimbus System", 
                Description = "A computer system used by the people working with us, it is an internal system" 
            };
            context.SupportAreas.Add(nimbusArea);
            context.SaveChanges();

            // Create PlayWrightRequest
            var playWrightRequest = new Support.Common.Models.PlayWrightRequest
            {
                Url = "http://localhost:8010/",
                WebTasks = new List<Support.Common.Models.WebTask>
                {
                    new Support.Common.Models.WebTask
                    {
                        ActionType = Support.Common.Models.WebTaskType.Click,
                        ElementId = "coloured",
                        SpeechText = "The first thing we do is click on 'Coloured Boxes'"
                    },
                    new Support.Common.Models.WebTask
                    {
                        ActionType = Support.Common.Models.WebTaskType.Click, 
                        ElementId = "green",
                        SpeechText = "Now we are going to click on the green box."
                    }
                }
            };
            context.PlayWrightRequests.Add(playWrightRequest);
            context.SaveChanges();

            // Create Specific Issue
            var greenBoxIssue = new SpecificIssue 
            { 
                SupportAreaId = nimbusArea.Id, 
                Name = "Green Box", 
                Description = "The user wants to click the Green box but doesn't know how to" 
            };
            context.SpecificIssues.Add(greenBoxIssue);
            context.SaveChanges();

            // Create Solution
            var greenBoxSolution = new Solution
            {
                SpecificIssueId = greenBoxIssue.Id,
                Name = "Click on Green Box",
                Description = "You will be shown how to click on the Green Box",
                Request = playWrightRequest.Id
            };
            context.Solutions.Add(greenBoxSolution);
            context.SaveChanges();
        }
    }
}

