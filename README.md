A lightweight, mobile-friendly reader for the TLDR newsletters. Built with .NET MAUI 9 (Android), it pulls the latest RSS feeds and presents them in clean, card-style lists with day dividers, category color accents, and a manual light/dark mode toggle.

⚠️ This is an unofficial client. All content belongs to the TLDR team.
RSS endpoint: https://tldr.tech/api/rss/<category>

Features

Categories: Tech / AI / Design / Crypto (tap chips to switch)

Auto-load on open + Pull-to-refresh

Day grouping with nice headers (e.g., “Friday, 17 Oct 2025”)

Cards: title, date, and actions — Open, Share, Copy link

Light/Dark toggle (top-right button) — cards stay readable in both

Category-tinted gradient background (subtle + pretty)

Offline-ish: best-effort local cache (used while fresh content loads)

Haptic feedback (optional) on certain actions (Android)

Screenshots
![Media (4)](https://github.com/user-attachments/assets/5c66b21f-491e-41ee-abd0-dbe7250bdd62)
![Media (5)](https://github.com/user-attachments/assets/8d52b44f-8ec4-4eb5-bd95-58c65fb72b1b)
![Media (6)](https://github.com/user-attachments/assets/8c1d4ceb-11d6-42de-b312-088f48c7e385)

Tech Stack

.NET MAUI 9 (Android target)

C# 12 / .NET 9

System.ServiceModel.Syndication for parsing RSS

MVVM with a simple FeedViewModel and FeedService

App icon & splash via Maui assets

No CommunityToolkit dependency (kept things lean & version-safe).
