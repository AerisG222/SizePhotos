# Version History

## 0.7.1 (10/26/2023)

- use a new pre-defined rawtherapee processing profile (Standard Film Curve ISO Low) which addresses an issue where some colors in photos were washed out
- update test scripts to reference .net7 build of application
- update container to use Fedora 38
- updated ImageSharp

## 0.7.0 (03/14/2023)

- updated to .net 7
- removed a number of dependencies (imagemagick/magickwand, jpegtran, jpegoptim)
- using SixLabors.ImageSharp for processing
- significantly refactored code to be more straightforward

## 0.6.0

- never published

## 0.5.0 (02/22/2019)

- update process to produce a new scale size (xs_sq), which scales+crops to always generate a consistent 160x120 thumbnail (wxh)
- add new metadata to the db (file size data, and new scale size)

## 0.4.0 (12/20/2018)

- update nexiftool

## 0.3.1 (09/23/2018)

- update libs / dependencies

## 0.3.0 (06/28/2016)

- update to rtm / latest deps

## 0.2.0 (03/05/2016)

- support creation of insert and update sql scripts
- update set of exif data that is stored
- update dependencies

## 0.1.1 (03/05/2016)

- update to use the nmagickwand wrapper

## 0.1.0 (02/13/2016)

- initial release
