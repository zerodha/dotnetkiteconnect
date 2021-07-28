
build:
	@dotnet build -c Release KiteConnect/KiteConnect.csproj

pack:
	@dotnet pack -c Release KiteConnect/KiteConnect.csproj

clean:
	@rm -rf KiteConnect/bin
	@dotnet clean

docs:
	@cp KiteConnect/bin/Release/netstandard2.0/KiteConnect.xml Documentation/kiteconnect.xml
	cd Documentation; python process.py
	cp CHANGELOG.md Documentation/docs/changelog.md

test:
	@dotnet test