#!/bin/bash
dotnet run -p ../src/SizePhotos/project.json SizePhotos -q -u -o x.sql -p update_test/2015/testdir -w /img
