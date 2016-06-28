#!/bin/bash
dotnet run -p ../src/SizePhotos/project.json SizePhotos -q -i -c test -o x.sql -p testfiles -w img -y 2016
