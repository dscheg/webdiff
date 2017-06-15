# WebDiff
WebDiff is a configurable .NET command-line tool for comparing two sites by list of URLs.

### Features
* Firefox, Chrome support
* Full page screenshots (vertical scrolling)
* Window sizes, Chrome mobile emulation
* Dynamic content waiting
* Injecting CSS/JS [WebExtension]
* Modifying HTTP-headers [WebExtension]
* Cookies
* HTML report with compare results

### Get
```
git clone https://github.com/dscheg/webdiff
cd webdiff
build.bat
```
or
1. Use IDE to build `*.sln`
2. Copy `cfg\*` to `bin\`
3. Copy `ext\bin\webdiff.crx` to `bin\`
4. Download and unzip [ChromeDriver](https://chromedriver.storage.googleapis.com/2.30/chromedriver_win32.zip) to `bin\`
  ...
5. Configure your `profile.toml` and enjoy

### Run
```
Usage: webdiff [OPTIONS] URL1 URL2 [FILE]
Options:
  -o, --output=VALUE         Reports output directory
                               (default: '.')
  -p, --profile=VALUE        Profile TOML file with current settings
                               (default: 'profile.toml')
  -t, --template=VALUE       HTML report template file
                               (default: 'template.html')
  -h, --help                 Show this message

Examples:
  webdiff http://prod.example.com http://test.example.com < URLs.txt
  webdiff -p profile.toml -t template.html -o data http://prod.example.com http://test.example.com URLs.txt
  ```
