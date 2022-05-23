FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build

WORKDIR /src

COPY SizePhotos.sln .
COPY pp3/. ./pp3/
COPY src/. ./src/

RUN dotnet restore
RUN dotnet publish -o /app -c Release -r linux-x64 --no-self-contained


# build runtime image
FROM fedora:36

RUN dnf install -y \
    dotnet-runtime-6.0 \
    perl-Image-ExifTool \
    jpegoptim \
    libjpeg-turbo-utils \
    ImageMagick-devel \
    rawtherapee \
  	&& dnf clean all \
  	&& rm -rf /var/cache/yum

WORKDIR /size-photos

COPY --from=build /app .

ENTRYPOINT [ "/size-photos/SizePhotos" ]