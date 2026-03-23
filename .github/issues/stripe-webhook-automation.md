# Automate License Key Generation on Stripe Purchase

## Priority: Medium
## Status: Planned

## Requirements
- When a customer completes a Stripe checkout, automatically:
  1. Generate a valid license key (Pro or Enterprise based on product)
  2. Email the key to the customer
  3. Log the purchase for records

## Proposed Architecture
- **Cloudflare Worker** — receives Stripe webhook, generates key, sends email
- **Resend** (resend.com) — free tier email delivery from noreply@arcadiaui.com
- **Stripe webhook** — checkout.session.completed event

## Setup Needed
1. Resend account + API key
2. Resend domain verification for arcadiaui.com
3. Stripe webhook endpoint configuration
4. Cloudflare Worker deployment

## Key Generation
Use same checksum algorithm as ArcadiaLicense.cs:
- ARC-P prefix for Pro ($299)
- ARC-E prefix for Enterprise ($799)
- Groups 1+2 random alphanumeric, group 3 is checksum

## Email Template
Subject: Your Arcadia Controls License Key
Body: Key, installation instructions, docs link, support contact
