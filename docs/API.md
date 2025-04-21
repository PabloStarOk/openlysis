# API Quick Reference

- API URL: `https://[domain]/api/v1`

## Authentication

All endpoints require an API key in a header.

- Header Name: 'X-Api-Key'
- Value: An api key.

### Sign Up

- HTTP method: `POST`
- Full endpoint path: `https://[domain]/api/v1/auth/sign-up`
- Endpoint path: `/auth/sign-up`
- API Key required: No

### Sign In

- HTTP method: `POST`
- Full endpoint path: `https://[domain]/api/v1/auth/sign-in`
- Endpoint path: `/auth/sign-in`
- API Key required: No

### Api Key Reset

- HTTP method: `POST`
- Full endpoint path: `https://[domain]/api/v1/auth/api-key-reset`
- Endpoint path: `/auth/api-key-reset`
- API Key required: No

## Files

### Upload to analyze a file

- HTTP method: `POST`
- Full endpoint path: `https://[domain]/api/v1/files`
- Endpoint path: `/files`
- API Key required: Yes

### Get file multi analysis by ID

- HTTP method: `GET`
- Full endpoint path: `https://[domain]/api/v1/files/analyses/{id}`
- Endpoint path: `/files/analyses/{id}`
- API Key required: Yes

### Get file multi analysis by hash

- HTTP method: `GET`
- Full endpoint path: `https://[domain]/api/v1/files/{hash}/analyses`
- Endpoint path: `/files/{hash}/analyses`
- API Key required: Yes

## URLs

### Upload to analyze an URL

- HTTP method: `POST`
- Full endpoint path: `https://[domain]/api/v1/urls`
- Endpoint path: `/urls`
- API Key required: Yes

### Get URL multi analysis by ID

- HTTP method: `GET`
- Full endpoint path: `https://[domain]/api/v1/urls/analyses/{id}`
- Endpoint path: `/urls/analyses/{id}`
- API Key required: Yes

### Get URL multi analysis by hash

- HTTP method: `GET`
- Full endpoint path: `https://[domain]/api/v1/urls/{hash}/analyses`
- Endpoint path: `/urls/{hash}/analyses`
- API Key required: Yes

## Phone Numbers

### Sends a phone number to get its reputation

- HTTP method: `GET`
- Full endpoint path: `https://[domain]/api/v1/phone-numbers/{phone-number}`
- Endpoint path: `/phone-numbers/{phone-numbers}`
- API Key required: Yes

## Email Address

### Sends an email address to get its reputation

- HTTP method: `GET`
- Full endpoint path: `https://[domain]/api/v1/email-addresses/{email-address}`
- Endpoint path: `/email-addresses/{email-address}`
- API Key required: Yes

## SMS

### Sends an SMS message to be analyzed

- HTTP method: `POST`
- Full endpoint path: `https://[domain]/api/v1/sms`
- Endpoint path: `/sms`
- API Key required: Yes

### Gets an SMS analysis by Id

- HTTP method: `GET`
- Full endpoint path: `https://[domain]/api/v1/sms/analyses/{id}`
- Endpoint path: `/sms/analyses/{id}`
- API Key required: Yes

### Get SMS analyses by hash

- HTTP method: `GET`
- Full endpoint path: `https://[domain]/api/v1/sms/{hash}/analyses`
- Endpoint path: `/sms/{hash}/analyses`
- API Key required: Yes

## Emails

### Gets an Email analysis by Id

- HTTP method: `GET`
- Full endpoint path: `https://[domain]/api/v1/email/analyses/{id}`
- Endpoint path: `/email/analyses/{id}`
- API Key required: Yes
