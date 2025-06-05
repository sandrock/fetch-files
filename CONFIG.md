
Configuring FetchFiles
==============================

Configuration file format
----------------------------

A configuration file for FetchFiles is a JSON.

```json
{
  "__manifest": "FetchFilesConfiguration",
  "Units": { },
  "Stores": { }
}
```


Unit type `Local`
----------------------------

This unit will search for files in a local directory.

The `Location` entry is mandatory and must be a valid directory. 

The `FileFormat` is an optional regular expression allowing to select the files to fetch.

```json
{
  "Type": "Local",
  "Description": "on local computer, myservice produces backup files",
  "Location": "/tmp/fetchfiles-test/myservice",
  "FileFormat": "^myservice_(?<id>[0-9T-]+)_(?:.+)\\.zip$"
}
```


Store type `Local`
----------------------------

This store will keep files in a local directory.

The `Location` entry is mandatory and must be a valid directory.

```json
{
  "Type": "Local",
  "Location": "/tmp/fetchfiles-test/store1"
}
```

