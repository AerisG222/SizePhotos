[![MIT licensed](https://img.shields.io/badge/license-MIT-blue.svg)](https://github.com/AerisG222/SizePhotos/blob/master/LICENSE.md)
[![Travis](https://img.shields.io/travis/AerisG222/SizePhotos.svg)](https://travis-ci.org/AerisG222/SizePhotos)
[![Coverity Scan](https://img.shields.io/coverity/scan/7996.svg)](https://scan.coverity.com/projects/aerisg222-sizephotos)

# SizePhotos

This is a small utility I use to prepare photos for my website.
It processes raw + jpg files in the following ways:

- converts RAW to JPG
- creates a few different photo sizes
- reads the exif data
- prepares a sql script that can make the photos available on the site

## Motivation
I enjoy taking photos, and combining this with development makes a
rewarding hobby for myself.  I have been using a version of this tool
for many years, and thought it might be useful for others to use directly
or as a starting point for more specific needs.

## Using
`dnx run SizePhotos -c "category" -o c.sql -p /home/user/Desktop/pictures/ -w /images -y 2016`

Arguments:
- c = name of the category
- o = path to sql file to generate
- p = directory containing the photos
- w = path to the root of this folder for the website
- x = private
- y = year

### Running via podman

```
podman run -it --rm --security-opt label=disable -v /home/mmorano/maw_test/rawtherapee-config/:/config -v /home/mmorano/Desktop/:/src --env-file /home/mmorano/maw_test/podman-env/rawtherapee.env localhost/maw-size-photos-test -c 'Comet Pond' -p /src/size_photos_test -w /images -r 'friend admin' -y 2022 -i -o /src/test_comet_pond.sql
```

we map a config directory so we can customize values in camconst.json - which is currently needed for z6ii.

rawtherapee.env:

```
RT_SETTINGS=/config
```

camconst.json (in config dir):

```
{"camera_constants": [
	{ // Quality C, only color matrix and PDAF lines info
		"make_model" : "Nikon Z 6_2",
		"dcraw_matrix" : [8210, -2534, -683, -5355, 13338, 2212, -1143, 1928, 6464], // DNG v13.2
		"pdaf_pattern" : [0, 12],
		"pdaf_offset" : 32
	}
]}
```

## Contributing
I'm happy to accept pull requests.  By submitting a pull request, you
must be the original author of code, and must not be breaking
any laws or contracts.

Otherwise, if you have comments, questions, or complaints, please file
issues to this project on the github repo.

## License
SizePhotos is licensed under the MIT license, see LICENSE.md for more
information.
