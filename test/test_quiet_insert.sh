#!/bin/bash
dotnet ../src/SizePhotos/bin/Debug/netcoreapp2.2/SizePhotos.dll -q -i -c test -o x.sql -p testfiles -w img -y 2016
