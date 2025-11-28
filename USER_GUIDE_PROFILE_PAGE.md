# User Profile Page Guide

## Table of Contents
1. [Overview](#overview)
2. [Accessing Your Profile](#accessing-your-profile)
3. [Profile Features](#profile-features)
4. [Sharing Your User ID](#sharing-your-user-id)
5. [My Leagues Section](#my-leagues-section)
6. [My Teams Section](#my-teams-section)
7. [Joining a League](#joining-a-league)
8. [FAQ](#faq)

---

## Overview

The User Profile page is your personal hub in Gridiron. Here you can:

- View your account information
- Copy your unique User ID to share with commissioners
- See all leagues you belong to
- View your roles (Commissioner, General Manager)
- Quick-access your teams and leagues

---

## Accessing Your Profile

### From the Navigation Bar

1. Click **"Profile"** in the top navigation bar
2. You'll be taken directly to your profile page at `/profile`

### Direct URL

Navigate to: `https://[your-domain]/profile`

> **Note:** You must be signed in to access your profile. If you're not authenticated, you'll be redirected to the login page.

---

## Profile Features

### Account Information Section

Your profile displays the following information:

| Field | Description |
|-------|-------------|
| **User ID** | Your unique identifier in the Gridiron system |
| **Display Name** | Your name as shown to other users |
| **Email** | Your email address (from Azure AD) |
| **Member Since** | Date you first signed up |
| **Last Login** | Most recent login timestamp |

### Global Administrator Badge

If you're a Global Administrator (God role), you'll see a purple badge:

```
🔧 Global Administrator
```

This indicates you have full system access to all leagues and teams.

---

## Sharing Your User ID

### What is Your User ID?

Your **User ID** is a unique number that identifies you in the Gridiron system. It's prominently displayed at the top of your profile in a highlighted box.

### How to Copy Your User ID

1. Navigate to your Profile page
2. Find the **"Your User ID"** section (highlighted in green/teal)
3. Click the **"Copy"** button next to your ID
4. The button will briefly show **"✓ Copied!"** to confirm

### Why Share Your User ID?

To join a league, you need to share your User ID with a league commissioner. The commissioner will use this ID to:

- Invite you to their league
- Assign you as a General Manager (GM) of a team
- Grant you Commissioner access to help manage the league

### Example Workflow

```
You: "Hey, I'd like to join your NFL 2025 league!"
Commissioner: "Sure! What's your User ID?"
You: "My User ID is 42"
Commissioner: [Assigns you as GM of the Cowboys]
You: [Now you see Cowboys in your My Teams section]
```

---

## My Leagues Section

### What You'll See

The **My Leagues** section shows all leagues where you have a role:

- **League Name** - The name of each league
- **Role Badges** - Your role(s) in that league:
  - 👑 **Commissioner** - You manage the entire league
  - 📋 **GM** - You manage a specific team (team name shown)

### League Count

The section header shows your total number of leagues: `My Leagues (3)`

### Clicking a League

Click on any league card to navigate to that league's detail page where you can:
- View the league structure
- See all teams and standings
- Manage teams (if you're Commissioner)

### Empty State

If you haven't joined any leagues yet, you'll see:

```
🏈
No leagues yet
Join a league by sharing your User ID with a commissioner, or create your own league
```

---

## My Teams Section

### When This Section Appears

The **My Teams** section only appears if you're a General Manager of at least one team.

### What You'll See

For each team you manage:
- **Team Name** - The name of your team
- **League Name** - Which league the team belongs to

### Team Count

The section header shows: `My Teams (2)`

### Clicking a Team

Click on any team card to navigate directly to that team's detail page where you can:
- View your roster
- Manage depth charts
- View player statistics

---

## Joining a League

### Step 1: Get Your User ID

1. Go to your Profile page
2. Copy your User ID (click the Copy button)

### Step 2: Contact a Commissioner

Share your User ID with a league commissioner through:
- Discord
- Email
- In-person conversation
- Any communication method

### Step 3: Wait for Assignment

The commissioner will assign you a role in their league. Once assigned:
- The league will appear in your **My Leagues** section
- If assigned as GM, your team will appear in **My Teams**

### Step 4: Refresh Your Profile

After the commissioner assigns you:
1. Refresh your profile page
2. Your new league/team will now be visible

---

## FAQ

### Q: Why can't I see any leagues?

**A:** You need to be assigned a role (Commissioner or GM) by someone who has permission to do so. Share your User ID with a commissioner to get invited.

### Q: Can I be in multiple leagues?

**A:** Yes! You can have roles in unlimited leagues. You can even be a Commissioner in one league and a GM in another.

### Q: Can I be GM of multiple teams?

**A:** Yes! You can manage multiple teams across different leagues. Each team will appear in your My Teams section.

### Q: How do I become a Commissioner?

**A:** Two ways:
1. **Create a new league** - You automatically become Commissioner
2. **Get promoted** - A Global Admin or existing Commissioner can assign you the Commissioner role

### Q: Can I change my display name or email?

**A:** Your display name and email come from your Azure AD account. To change them, update your profile in Azure AD (or contact your system administrator).

### Q: What if my User ID doesn't work?

**A:** Ensure:
1. You're signed in when viewing your profile
2. The commissioner is entering your ID correctly
3. The commissioner has permission to assign roles

### Q: How do I leave a league?

**A:** Contact the league Commissioner and ask them to remove your role. Once removed, the league will disappear from your profile.

### Q: What's the difference between Commissioner and GM?

| Role | Access Level |
|------|-------------|
| **Commissioner** | Full access to ALL teams in the league. Can assign/remove GM roles. |
| **General Manager (GM)** | Access only to YOUR assigned team(s). Can manage roster and depth charts. |

---

## Related Documentation

- [Authorization and User Management](AUTHORIZATION_AND_USER_MANAGEMENT.md) - Technical details about the role system
- [Authentication Setup](../AUTHENTICATION_SETUP.md) - How sign-in works with Azure AD

---

## Support

If you encounter issues with your profile:

1. Try signing out and signing back in
2. Clear your browser cache
3. Contact a Global Administrator for assistance

---

*Last updated: November 2025*
