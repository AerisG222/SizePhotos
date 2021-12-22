#!/bin/bash
dotnet ../src/SizePhotos/bin/Debug/net5.0/SizePhotos.dll -q -i -c test -o x.sql -p testfiles -w img -y 2016 -r admin -r friend
