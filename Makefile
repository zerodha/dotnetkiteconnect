VERSION := $(shell grep -m1 '<Version>' KiteConnect/KiteConnect.csproj | sed -E 's/.*<Version>(.*)<\/Version>.*/\1/')

build:
	@dotnet build -c Release KiteConnect/KiteConnect.csproj

pack: build clean
	@dotnet pack -c Release KiteConnect/KiteConnect.csproj

clean:
	@rm -rf KiteConnect/bin
	@dotnet clean

docs:
	@cp KiteConnect/bin/Release/net10.0/KiteConnect.xml Documentation/kiteconnect.xml
	cd Documentation; uv run --with xmltodict process.py
	cp CHANGELOG.md Documentation/docs/changelog.md

test:
	@dotnet test

# setup: security add-generic-password -a "$USER" -s nuget-api-key -w
publish: pack
	@dotnet nuget push \
		"KiteConnect/bin/Release/Tech.Zerodha.KiteConnect.$(VERSION).nupkg" \
		--api-key "$$(security find-generic-password -s nuget-api-key -w)" \
		--source https://api.nuget.org/v3/index.json \
		--skip-duplicate