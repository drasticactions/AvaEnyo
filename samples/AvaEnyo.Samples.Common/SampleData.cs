using System;
using System.Collections.Generic;
using System.Linq;

namespace AvaEnyo.Samples;

/// <summary>
/// Deterministic fake data for the gallery and the demos: contacts, mail accounts, folders and messages, and photo
/// albums over the generated placeholder photos.
/// </summary>
public static class SampleData
{
    private static readonly string[] s_names =
    [
        "Ada Lovelace", "Brian Kernighan", "Carol Shaw", "Dennis Ritchie", "Edsger Dijkstra", "Frances Allen",
        "Grace Hopper", "Hedy Lamarr", "Ivan Sutherland", "Jean Bartik", "Ken Thompson", "Lynn Conway",
        "Margaret Hamilton", "Niklaus Wirth", "Olivia Chen", "Paul Graham", "Radia Perlman", "Sophie Wilson",
        "Tim Berners-Lee", "Ursula Martin", "Vint Cerf", "Wendy Hall", "Xavier Leroy", "Yukihiro Matsumoto",
    ];

    private static readonly string[] s_subjects =
    [
        "Lunch on Thursday?", "Slides for the review", "Re: The sliding panels demo", "Photos from the trip",
        "Your order has shipped", "Weekly sync notes", "Quick question about the build", "Tickets for Saturday",
        "Re: Re: Dinner plans", "Draft of the proposal", "Welcome to the team!", "Server maintenance tonight",
        "Book club: next pick", "Can you review my change?", "Holiday schedule", "Running late",
    ];

    private static readonly string[] s_bodies =
    [
        "Hi,\n\nAre you free for lunch on Thursday? There is a new place near the office that does great noodles.\n\nCheers",
        "Attached are the slides for tomorrow. I moved the motion section to the end and added the physics plots.\n\nLet me know what you think.",
        "The views now slide over each other in 700 ms with a cubic ease-out. Drag the middle one left and the stack follows.",
        "Here are a few of my favorites from the coast. The light at sunset was unreal.\n\nMore to come once I sort them.",
        "Good news: your order is on its way. You can track it from your account page.",
        "Notes from today:\n- Picker popups are done\n- Lists recycle rows with dividers\n- Next: the photo grid",
        "The CI run fails on the AOT step for me. Did you see the same warning, or is it my machine?",
        "I got two tickets for Saturday night. Want to come along? Doors open at seven.",
        "Sounds good. Let's say 7:30 at the usual place, and I'll book a table for four.",
        "Here is the first draft. The budget section still needs numbers from finance.",
        "We are glad to have you on board. Your first week is mostly setup and meeting the team.",
        "The mail server will be down from 11 pm to 1 am for upgrades. Sorry for the trouble.",
        "Next month's book is a short one, so no excuses this time.",
        "Small change to the scroller fades. It only touches two files.",
        "The office is closed from the 24th to the 2nd. Enjoy the break!",
        "Stuck in traffic, I'll be about ten minutes late. Start without me.",
    ];

    /// <summary>The fixed "today" of the sample data, so screenshots do not change from day to day.</summary>
    public static readonly DateTime Today = new(2011, 7, 1);

    public static IReadOnlyList<Contact> Contacts { get; } =
        s_names.Select((n, i) => new Contact(i, n, n.ToLowerInvariant().Replace(' ', '.').Replace("-", string.Empty) + "@example.com", i)).ToArray();

    public static IReadOnlyList<MailAccount> Accounts { get; } =
    [
        new MailAccount("Work", "me@work.example.com",
        [
            new MailFolder("inbox", "Inbox", 5, 60),
            new MailFolder("drafts", "Drafts", 0, 2),
            new MailFolder("sent", "Sent", 0, 34),
            new MailFolder("archive", "Archive", 0, 412),
            new MailFolder("trash", "Trash", 0, 9),
        ]),
        new MailAccount("Personal", "me@home.example.com",
        [
            new MailFolder("p-inbox", "Inbox", 2, 24),
            new MailFolder("p-travel", "Travel", 0, 11),
            new MailFolder("p-receipts", "Receipts", 0, 48),
        ]),
    ];

    /// <summary>Messages of every folder, newest first, generated from a fixed seed.</summary>
    public static IReadOnlyList<MailMessage> Messages { get; } = MakeMessages();

    /// <summary>Messages of one folder, newest first.</summary>
    public static IReadOnlyList<MailMessage> MessagesIn(string folderId) => Messages.Where(m => m.FolderId == folderId).ToArray();

    public static IReadOnlyList<PhotoAlbum> Albums { get; } =
    [
        new PhotoAlbum("Camera Roll", Enumerable.Range(0, 60).ToArray()),
        new PhotoAlbum("Coast", Enumerable.Range(0, 60).Where(i => i % 6 == 0 || i % 6 == 4).ToArray()),
        new PhotoAlbum("Mountains", Enumerable.Range(0, 60).Where(i => i % 6 == 2 || i % 6 == 5).ToArray()),
        new PhotoAlbum("Evenings", Enumerable.Range(0, 60).Where(i => i % 6 == 1 || i % 6 == 3).ToArray()),
    ];

    /// <summary>Simple words for list and picker samples.</summary>
    public static IReadOnlyList<string> Fruits { get; } =
    [
        "Apple", "Apricot", "Banana", "Blackberry", "Blueberry", "Cherry", "Coconut", "Date", "Fig", "Grape",
        "Guava", "Kiwi", "Lemon", "Lime", "Lychee", "Mango", "Melon", "Nectarine", "Orange", "Papaya", "Peach",
        "Pear", "Pineapple", "Plum", "Pomegranate", "Raspberry", "Strawberry", "Tangerine", "Watermelon",
    ];

    private static IReadOnlyList<MailMessage> MakeMessages()
    {
        var rng = new Random(42);
        var list = new List<MailMessage>();
        var id = 0;
        foreach (var account in Accounts)
        {
            foreach (var folder in account.Folders)
            {
                var count = Math.Min(folder.Total, 40);
                var time = Today.AddHours(17);
                for (var i = 0; i < count; i++)
                {
                    time = time.AddMinutes(-(20 + rng.Next(600)));
                    var subject = s_subjects[rng.Next(s_subjects.Length)];
                    list.Add(new MailMessage
                    {
                        Id = id++,
                        FolderId = folder.Id,
                        From = Contacts[rng.Next(Contacts.Count)],
                        Subject = subject,
                        Body = s_bodies[rng.Next(s_bodies.Length)],
                        Received = time,
                        IsUnread = i < folder.Unread,
                        IsFlagged = rng.Next(10) == 0,
                    });
                }
            }
        }

        return list;
    }
}
