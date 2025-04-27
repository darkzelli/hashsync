# HashSync

HashSync is a cross-platform desktop application that automatically syncs files between devices by detecting file changes and securely transferring updated files.

## Features

- Automatic File Syncing: Instantly updates files across devices when changes are detected.
- Cross-Platform Support: Runs on macOS, Windows, and Linux.
- Secure Change Detection: Monitors file changes using SHA-256 hashing.
- Customizable Settings: Configure tracked folders, scan intervals, and device permissions.
- Cloud Storage Integration: Uploads updated files to Supabase for secure multi-device access.

## How It Works

1. Monitor Files: HashSync generates a SHA-256 hash of tracked files at regular intervals.
2. Detect Changes: Compares new and previous hashes to identify any updates.
3. Sync Files: Uploads changed files to Supabase, where authorized devices can download them.
4. Link Devices: Devices are linked through a UUID generated during initial setup.

## UI Overview
![UI](https://i.imgur.com/CiGWBqg.png)
### Client Side

- Generates a UUID on first launch.
- Displays a countdown timer until the next file check.
- Allows users to pick folders and view full directories via depth-first search.
- Manages allowed devices for syncing.

### Server Side

- Set intervals to check for new downloadable files.
- Specify a download location.
- View detailed download history.
