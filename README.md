
# ShowHashes

![License](https://img.shields.io/badge/License-GPLv3-blue.svg)

## What

This is a program to view a given file's hashes.

![1](.github/shs-screenshot-1.png)

## Why

Show Hashes is a tool I wanted to have in order to quickly check, search and copy hashes in Windows 11

## Features

- **Supported hash methods**: CRC32, MD5, SHA1, SHA256.
- **Copy to clipboard**: Copy individual hashes or all the enabled ones.
- **Browser search**: Support to search individual or all the hashes in different browser engines: Google, Bing, DuckDuckGo, Yandex.
- **Whitelist, blacklist and single selection**: You can choose which hashes you want, by whitelisting, blacklisting or selecting individually
- **Toggle hash method**: YOu have the option to inlcude or nor the hash method when copying or searching.
- **Separator**: YOu have the option to set a custom separator when copying or searching.
- **Multilingual Support**: Available in multiple languages, including English, Español, Deutsch, Português, Français, Italiano, 日本語, 한국어, 中文, हिन्दी, and Русский.


## Build

Show Hashes is a Windows Presentation Foundation application which requires [.NET 8.0](https://dotnet.microsoft.com/en-us/download/dotnet/8.0).

To build the application for different architectures, use the following commands:
- **64 bit**: `dotnet publish -r win-x64 -p:PublishSingleFile=true --self-contained false`
- **32 bit**: `dotnet publish -r win-x86 -p:PublishSingleFile=true --self-contained false`
- **ARM**: `dotnet publish -r win-arm64 -p:PublishSingleFile=true --self-contained false`

---

![image](https://img.shields.io/badge/.NET-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)
![image](https://img.shields.io/badge/C%23-239120?style=for-the-badge&logo=csharp&logoColor=white)
![image](https://img.shields.io/badge/Visual_Studio-5C2D91?style=for-the-badge&logo=visual%20studio&logoColor=white)
