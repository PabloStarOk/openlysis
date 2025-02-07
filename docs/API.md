# API Endpoints

## Summary

- File
  - `POST` `/api/analyses/file` Submits a file to be analyzed.
  - `GET` `/api/analyses/file/{id}` Gets a file analysis by its id.
  - `GET` `/api/analyses/file/hash/{md5, sha1, sha256, sha512}` Gets a file analysis by hash.
- URL
  - `POST` `/api/analyses/url` Submits an URL to be analyzed.
  - `GET` `/api/analyses/url/{id}` Gets an URL analysis.

### File

- **`POST`** `/api/analyses/file`: Submits a file to be analyzed and gets a `FileAnalysis` ID.
- `GET` `/api/analyses/file/{id}` Gets a `FileAnalysis` with the following structure:

### `FileAnalysis` object

```json
{
    "id": "string",
    "lastScanDate": "Datetime",
    "reportsAmount": "int",
    "verdict": "undetected" | "suspicious" | "malicious",
    "file": {
        "hashSet": {
            "md5": "string",
            "sha1": "string",
            "sha256": "string",
            "sha512": "string"
        },
        "info": {
            "name": "string",
            "mimeType": "string",
            "size": "int",
            "creationDate": "Datetime",
        }
    },
    "reports": [
        {
            "id": "string",
            "serviceName": "string",
            "scanState": "queued" | "started" | "finished" | "timeout",
            "scanStartDate": "Datetime",
            "scanEndDate": "Datetime?",
            "verdict": "string",
            "detectionInfo": {
                "isEmpty": "bool",
                "type": "string",
                "zone":  "none" | "green" | "yellow" | "red"
            }
        }
    ]
}
```
