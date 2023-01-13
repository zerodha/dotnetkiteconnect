
build:
	@dotnet build -c Release KiteConnect/KiteConnect.csproj

pack:
	@dotnet pack -c Release KiteConnect/KiteConnect.csproj

clean:
	@rm -rf KiteConnect/bin
	@dotnet clean

docs:
	@cp KiteConnect/bin/Release/net6.0/KiteConnect.xml Documentation/kiteconnect.xml
	cd Documentation; python3 process.py
	cp CHANGELOG.md Documentation/docs/changelog.md

test:
	@dotnet test