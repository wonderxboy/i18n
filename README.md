# i18n (v2)
## Smart internationalization for ASP.NET
```
    PM> Install-Package I18N
```
### Introduction

The i18n library is designed to replace the use of .NET resources in favor 
of an **easier**, globally recognized standard for localizing ASP.NET-based web applications.

### Platforms Supported

i18n itself targets .NET Framework 4, and works with websites and web applications
based on ASP.NET v4 and above, including:

- ASP.NET MVC
- ASP.NET Web API
- ASP.NET WebMatrix / Web Pages
- ASP.NET Web Forms

### Features

- Leverages the [GetText / PO](http://en.wikipedia.org/wiki/Gettext) ecosystem: localize like the big kids
- Localize everything: HTML, Razor, C#, VB, JavaScript, .NET attributes and data annotations, ...
- SEO-friendly: language selection varies the URL, and `Content-Language` is set appropriately
- Automatic: no URL/routing changes required in the app
- High performance, minimal overhead and minimal heap allocations
- Smart: knows when to hold them, fold them, walk away, or run, based on i18n best practices

### Project Configuration

The i18n library works by modifying your HTTP traffic to perform string replacement and
patching of URLs with language tags ([URL Localization](#url-localization)). The work is done by an
HttpModule called i18n.LocalizingModule which should be enabled in your web.config file as follows:

```xml
  <system.web>
    <httpModules>
      <add name="i18n.LocalizingModule" type="i18n.LocalizingModule, i18n" />
    </httpModules>
  </system.web>
  <system.webServer> <!-- IIS7 'Integrated Mode'-specific config stuff -->
    <modules>
      <add name="i18n.LocalizingModule" type="i18n.LocalizingModule, i18n" />
    </modules>
  </system.webServer>
```

Note: The ```<system.webServer>``` element is added for completeness and may not be required.

The following ```<appSettings>``` are then required to specify the type and location 
of your application's source files:

```xml
  <appSettings>
    <add key="i18n.DirectoriesToScan" value=".." /> <!-- Rel to web.config file -->
    <add key="i18n.WhiteList" value="*.cs;*.cshtml;*.sitemap" />
  </appSettings>
```

Finally, certain behaviours of i18n may be altered at runtime on application startup. The following
code shows the most common options:

```csharp
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            // Change from the default of 'en'.
            i18n.LocalizedApplication.Current.DefaultLanguage = "fr";

            // Change from the of temporary redirects during URL localization
            i18n.LocalizedApplication.Current.PermanentRedirects = true;

            // This line can be used to disable URL Localization.
            //i18n.LocalizedApplication.Current.EarlyUrlLocalizerService = null;

            // Change the URL localization scheme from Scheme1.
            i18n.UrlLocalizer.UrlLocalizationScheme = i18n.UrlLocalizationScheme.Scheme2;

            // Blacklist certain URLs from being 'localized'.
            i18n.UrlLocalizer.IncomingUrlFilters += delegate(Uri url) {
                if (url.LocalPath.EndsWith("sitemap.xml", StringComparison.OrdinalIgnoreCase)) {
                    return false; }
                return true;
            };
        }
    }
```

### Usage
To localize text in your application, surround your strings with [[[ and ]]] markup
characters to mark them as translatable. That's it. 
Here's an example of localizing text in a Razor view:

```html
    <div id="content">
        <h2>[[[Welcome to my web app!]]]</h2>
        <h3><span>[[[Amazing slogan here]]]</span></h3>
        <p>[[[Ad copy that would make Hiten Shah fall off his chair!]]]</p>
        <span class="button" title="[[[Click to see plans and pricing]]]">
            <a href="@Url.Action("Plans", "Home", new { area = "" })">
                <strong>[[[SEE PLANS & PRICING]]]</strong>
                <span>[[[Free unicorn with all plans!]]]</span>
            </a>
        </span>
    </div>
```

And here's an example in an MVC controller:

```csharp
    using i18n;
    
    namespace MyApplication
    {
        public class HomeController : Controller
        {
            public ActionResult Index()
            {
                ViewBag.Message = "[[[Welcome to ASP.NET MVC!]]]";

                return View();
            }
        }
    }
```

At last, you can localize your data annotations as easy as this:

```csharp
    public class PasswordResetViewModel
    {
        [Required(ErrorMessage="[[[Please fill in this field]]]")]
        [Email(ErrorMessage = "[[[Email not yet correct]]]")]
        [Display(Name = "[[[Email Address]]]")]
        public string Email
        {
            get;
            set;
        }
    }
```

And localize arguments passed to MVC URL-Helpers or other functions that require a plain string:

```html
@Html.LabelFor(m => m.Name, "[[[First Name]]]")
```

And for Javascript:

```html
    <script type="text/javascript">
        $(function () {
            alert("[[[Hello world!]]]");
        });
    </script>
```

### Nuggets

In PO terminology, strings you want to be translatable are known as messages.
In i18n, messages are 'marked-up' in your source code as 'Nuggets'. The nugget markup allows 
i18n to filter the HTTP response looking for the
message strings which are replaced with translated strings, where available. They also allow
message strings to be located by the PostBuild PO file generator.

A simple nugget looks like this:

```
[[[translate me]]]
```

This defines a message with the key (aka msgid) of "translate me".

Nugget markup supports formated messages as follows:

```
string.Format("[[[welcome %1, today is %0|||{0}|||{1}]]]", day, name)
```

where the %0 and %1 tokens are replaced by the strings that replace the {0} and {1} items, respectively.
(The reason for the extra level of redirection here is to facilitate the translator rearranging the order of
the tokens for different languages.)

Nugget markup also supports comments (_extracted comments_ in PO parlance) to be passed to the translator like so:

```
[[[translate me///this is an extracted comment]]]
```

And if you need to include the markup characters themselves within a message, you can HTML-escape them, for example:

```
[[[Please don't forget to add GoogleAd tags: [googleadsmall&#93;]]]
```

where &amp;#93; is the HTML escape sequence for ]. The relevant escape sequences are:

- / = &amp;#47;
- [ = &amp;#91;
- ] = &amp;#93;
- | = &amp;#124;

See [Issue #50](https://github.com/turquoiseowl/i18n/issues/50) for more on Nuggets and why we have chosen to
replace the GetText / _() style of marking-up messages.

#### Nugget markup customization

The character sequences for marking-up nuggets ([[[, ]]], ||| and ///) were chosen on the basis that they were unlikely to clash with
common character sequences in HTML markup while at the same time being convenient for the programmer
to enter (on most keyboards).

However, recognizing that a clash remains possible and nuggets thereby being falsely detected
in source code or the HTML response, i18n allows you to define your own sequences for the markup
which you know are not going to clash. You can configure these in web.config as follows:

```xml
  <appSettings>
    ...
    <add key="i18n.NuggetBeginToken" value="[&[" />
    <add key="i18n.NuggetEndToken" value="]&]" />
    <add key="i18n.NuggetDelimiterToken" value="||||" />
    <add key="i18n.NuggetCommentToken" value="////" />
    ...
  </appSettings>
```

#### Message Context Support

i18n allows you to assign a ```msgctxt``` value to each message. Teh value of the msgctxt is
taken from any comment you have defined in the nugget. This feature is optional and disabled
by default. To enable this feature:


```xml
  <appSettings>
    ...
    <add key="i18n.MessageContextEnabledFromComment" value="true" />
    ...
  </appSettings>
```

Note that note all PO editors support msgctxt and indeed may be thrown by the value
when present in .PO files. See Issue #90 for more details.

### Building PO databases

To set up automatic PO database building, add the following post-build task to your project, after
adding `i18n.PostBuild.exe` as a project reference:

```
    "$(TargetDir)i18n.PostBuild.exe" "$(ProjectDir)\web.config"
```
    
Alternatively, you may choose to install the `i18n.POTGenerator.vsix` Visual Studio 2012 extension.
This installs an `i18n` button in the Solution Window for manual triggering of PO generation. Note that
it is necessary to highlight the project in question within the Solution Window before pressing the button.

The PO generator will rip through your source code (as defined by the
i18n.DirectoriesToScan and i18n.WhiteList settings in web.config), finding every nugget, 
and uses this to build a master .POT template file located at `locale/messages.pot`
relative to your web application folder. After the new template is constructed, any locales that exist 
inside the `locale` folder (or as defined by the i18n.AvailableLanguages semi-colon-delimited web.config setting)
are automatically merged with the template, so that new strings can be flagged for further translation.

From here, you can use any of the widely available PO editing tools (like [POEdit](http://www.poedit.net))
to provide locale-specific text and place them in your `locale` folder relative to the provided language, e.g. `locale/fr`. 
If you change a PO file on the fly, i18n will update accordingly; you do _not_ need to restart your application.

### URL Localization

In keeping with emerging standards for internationalized web applications, i18n provides support for
localized URLs. For example, `www.example.com/de` or `www.example.com/en-us/signin`.

Out of the box, i18n will attempt to ensure the current language for any request is shown correctly in the
address box of the user's browser, redirecting from any non-localized URL if necessary to a localized one.
This is known as [Early URL Localization](https://docs.google.com/drawings/d/1cH3_PRAFHDz7N41l8Uz7hOIRGpmgaIlJe0fYSIOSZ_Y/edit?pli=1).
See also [Principal Application Language](#principal-application-language).

While URLs from the user-agent perspective are localized, from the app's perspective they are nonlocalized.
Thus you can write your app without worrying about the language tag in the URL.

The default URL Localization scheme (Scheme1) will show the language tag in the URL always; an alternative
scheme, Scheme2, will show the language tag only if it is not the default. Alternatively, URL localization
can be disabled by setting `i18n.LocalizedApplication.Current.EarlyUrlLocalizerService = null` in `Application_Start`.

#### Exclude URLs from being localized

URLs to non-internationalized resources need not be localized. Typically, there
is no harm in them being localized as i18n will route the request approriately either
way. However, where the Principal Application Language for a request is not required,
such as for when reading a CSS file or font file, it can save a redirection round
trip by instructing i18n NOT to localize the URL.

There are two ways to instruct i18n NOT to localize a URL:

Firstly, you can set a RegEx pattern to match against the localpath part of the URLs to be excluded. For instance:

```
    protected void Application_Start()
    {
        ...
        // Blacklist certain URLs from being 'localized'.
        i18n.UrlLocalizer.QuickUrlExclusionFilter = new System.Text.RegularExpressions.Regex(@"(?:sitemap\.xml|\.css|\.jpg|\.png|\.svg|\.woff|\.eot)$");
    }
```

Indeed, the default value for the QuickUrlExclusionFilter settings is as shown above however
feel free to override or set to null to disable.

For finer control, the second method is to define filter delegates that are passed the URL and return
true if the URL is to be localized, otherwise false. For example:

```
    protected void Application_Start()
    {
        ...
        // Blacklist certain URLs from being 'localized'.
        i18n.UrlLocalizer.IncomingUrlFilters += delegate(Uri url) {
            if (url.LocalPath.EndsWith("sitemap.xml", StringComparison.OrdinalIgnoreCase)) {
                return false; }
            return true;
        };
        i18n.UrlLocalizer.OutgoingUrlFilters += delegate(string url, Uri currentRequestUrl) {
            Uri uri;
            if (Uri.TryCreate(url, UriKind.Absolute, out uri)
                || Uri.TryCreate(currentRequestUrl, url, out uri)) {
                if (uri.LocalPath.EndsWith("sitemap.xml", StringComparison.OrdinalIgnoreCase)) {
                    return false; }
            }
            return true;
        };
    }
```

### Principal Application Language

During startup of your ASP.NET application, i18n determines the set of application 
languages for which one or more translated messages exist.

Then, on each request, one of these languages is selected as the Principal Application 
Language (PAL) for the request.

The PAL for the request is determined by the first of the following conditions that is met:

For i18n.UrlLocalizationScheme.Scheme1:

1. The path component of the URL is prefixed with a language tag that matches *exactly* one of the application languages. E.g. "example.com/fr/account/signup".

2. The path component of the URL is prefixed with a language tag that matches *loosely* one of the application languages (see below).

3. The request contains a cookie called "i18n.langtag" with a language tag that matches (exactly or loosely) one of the application languages.

4. The request contains an Accept-Language header with a language that matches (exactly or loosely) one of the application languages.

5. The default application language is selected (see also [Per-Request Default Language Determination](#per-request-default-language-determination)).

For i18n.UrlLocalizationScheme.Scheme2:

1. The path component of the URL is prefixed with a language tag that matches *exactly* one of the application languages. E.g. "example.com/fr/account/signup".

2. The path component of the URL is prefixed with a language tag that matches *loosely* one of the application languages (see below).

3. The default application language is selected (see also [Per-Request Default Language Determination](#per-request-default-language-determination)).

Where a *loose* match is made above, the URL is updated with the matched application language tag
and a redirect is issued. E.g. "example.com/fr-CA/account/signup" -> "example.com/fr/account/signup".
By default this is a temporary 302 redirect, but you can choose for it to be a permanent 301 one
by setting `i18n.LocalizedApplication.Current.PermanentRedirects = true` in Application_Start.

The `GetPrincipalAppLanguageForRequest` extension method to HttpContext can be called to access the
PAL of the current request. For example, it may be called in a Razor view as follows to display
the current langue to the user:

```xml
    <div>
        <p id="lang_cur" title="@Context.GetPrincipalAppLanguageForRequest()">
            @Context.GetPrincipalAppLanguageForRequest().GetNativeNameTitleCase()
        </p>
    </div>
```

### Per-Request Default Language Determination

When the PAL algorithm falls back on the default language for the application, i18n supports a
simple delegate-based hook for providing the default language based on the current request,
typically based on the URL.

For example, suppose you wish the default language to vary as follows:

1. mydomain.co.uk -> 'en'
2. mydomain.fr -> 'fr'

This can be achieved as follows:

```
    protected void Application_Start()
    {
        ...
        i18n.LocalizedApplication.Current.DefaultLanguage = "en";
        i18n.UrlLocalizer.UrlLocalizationScheme = i18n.UrlLocalizationScheme.Scheme2;
        i18n.UrlLocalizer.DetermineDefaultLanguageFromRequest = delegate(HttpContextBase context)
        {
            if (context != null && context.Request.Url.Host.EndsWith(".fr", StringComparison.OrdinalIgnoreCase)) {
                return i18n.LanguageTag.GetCachedInstance("fr"); }
            return i18n.LocalizedApplication.Current.DefaultLanguageTag;
        };
    }
```

Notice how the URL localization scheme has been switched to Scheme2 which allows the URL
to be without any language tag. The default scheme (Scheme1) would enforce a redirection
so that the URL always contains the current language tag.

### Explicit User Language Selection

You probably want to allow users to override their browser language settings by providing a language selection
feature in your application. There are two parts to implementing this feature with i18n which revolve around
the setting of a cookie called `i18n.langtag`.

Firstly, you need to provide HTML that displays the current language and allows the user to explicitly select
a language (from those ApplicationLanguages available). An example of how to do that in ASP.NET MVC and Razor follows:

```xml
@using i18n
...
<div id="language">
  <div>
    <p id="lang_cur" title="@Context.GetPrincipalAppLanguageForRequest()">@Context.GetPrincipalAppLanguageForRequest().GetNativeNameTitleCase()</p>
  </div>
  <div id="lang_menu" style="display: none;">
    <table class="table_grid">
      <tbody>
        @{
          int i;
          int maxcols = 3;
          KeyValuePair<string, i18n.LanguageTag>[] langs = LanguageHelpers.GetAppLanguages().OrderBy(x => x.Key).ToArray();
          int cellcnt = langs.Length +1;
          for (i = 0; i < cellcnt;) {
            bool lastRow = i + maxcols >= cellcnt;
            <tr class="@(Html.Raw((i % 2) == 0 ? "even":"odd")) @(Html.Raw(lastRow ? "last":""))">
              @for (int j = 0; j < maxcols && i < cellcnt; ++i, ++j) {
                string langtag;
                string title;
                string nativelangname;
                if (i == 0) {
                  langtag = "";
                  title = "[[[Browser default language setting]]]";
                  nativelangname = "[[[Auto]]]";
                }
                else {
                  i18n.LanguageTag lt = langs[i -1].Value;
                  title = langtag = lt.ToString();
                  nativelangname = lt.NativeNameTitleCase;
                }
                <td>
                  @Html.ActionLink(
                    linkText: nativelangname, 
                    actionName: "SetLanguage", 
                    controllerName: "Account", 
                    routeValues: new { langtag = langtag, returnUrl = Request.Url },
                    htmlAttributes: new { title = title } )
                </td>
              }
              @* Fill last row with empty cells if ness, so that borders are added and balanced out. *@
              @if (lastRow) {
                for (; i % maxcols != 0; ++i) {
                  <td></td>
                }
              }
            </tr>
          }
        }
      </tbody>
    </table>
  </div>
</div>
```

On selection of a language in the above code, the AccountController.SetLanguage method is called, an example of
which follows:

```csharp

    using i18n;
    ...

    //
    // GET: /Account/SetLanguage

    [AllowAnonymous]
    public ActionResult SetLanguage(string langtag, string returnUrl)
    {
        // If valid 'langtag' passed.
        i18n.LanguageTag lt = i18n.LanguageTag.GetCachedInstance(langtag);
        if (lt.IsValid()) {
            // Set persistent cookie in the client to remember the language choice.
            Response.Cookies.Add(new HttpCookie("i18n.langtag")
            {
                Value = lt.ToString(),
                HttpOnly = true,
                Expires = DateTime.UtcNow.AddYears(1)
            });
        }
        // Owise...delete any 'language' cookie in the client.
        else {
            var cookie = Response.Cookies["i18n.langtag"];
            if (cookie != null) {
                cookie.Value = null;
                cookie.Expires = DateTime.UtcNow.AddMonths(-1);
            }
        }
        // Update PAL setting so that new language is reflected in any URL patched in the 
        // response (Late URL Localization).
        HttpContext.SetPrincipalAppLanguageForRequest(lt);
        // Patch in the new langtag into any return URL.
        if (returnUrl.IsSet()) {
            returnUrl = LocalizedApplication.Current.UrlLocalizerForApp.SetLangTagInUrlPath(HttpContext, returnUrl, UriKind.RelativeOrAbsolute, lt == null ? null : lt.ToString()).ToString(); }
        // Redirect user agent as approp.
        return this.Redirect(returnUrl);
    }

```

### How to get a translation of a nugget in your C# code

With i18n you can access the translation for a given nugget msgid from any code that is handling an ASP.NET request.
There is a ```GetText``` extension method to HttpContextBase provided for this.

For example, you can do the following from within an MVC controller action:


```
using System;
using System.Web.Mvc;
using i18n;

namespace MyWebSite.Controllers
{
    public class MyController : Controller
    {
        public ActionResult Welcome()
        {
            string welcomeMessage = HttpContext.GetText("Welcome to the my website.", "");

            // Do something with the string...

            return View();
        }
    }
}

```

Essentially, anywhere you have access to an HttpContextBase or HttpContext instance, you can get a correct
translation for a given nugget msgid / msgcomment combo.

The msgcomment is relevant only when i18n.Domain.Concrete.i18nSettings.MessageContextEnabledFromComment is set to true;
by default it is false and so msgcomment argument should be passed as null or empty.

### Language Matching

Language matching is performed when a list of one or more user-preferred languages is matched against
a list of one or more application laguages, the goal being to choose the application languages
which the user is most likely to understand. The algorithm for this is multi-facted and multi-pass and takes the Language, 
Script and Region subtags into account.

Matching is performed once per-request to determine the [Principal Application Language](#principal-application-language)
for the request, and also once per message to be translated (aka GetText call). 
The multi-pass approach ensures a thorough attempt is made at matching a user's list of preferred 
languages (from their Accept-Language HTTP header). E.g. in the context of the following request:

```
User Languages: fr-CH, fr-CA  
Application Languages: fr-CA, fr, en
```

*fr-CA* will be matched first, and if no resource exists for that language, *fr* is tried, and failing
that, the default language *en* is fallen back on.

In recognition of the potential bottleneck of the GetText call (which typically is called many times per-request),
the matching algorithm is efficient for managed code (lock-free and essentially heap-allocation free).

Note that the following Chinese languages tags are normalized: zh-CN to zh-Hans, and zh-TW to zh-Hant.
It is still safe to use zh-CN and zh-TW, but internally they will be treated as equivalent to their new forms.

##### Private Use Subtag

The [w3c language tag spec](http://www.w3.org/International/articles/language-tags/Overview.en.php#extension) includes 
a provision for an additional subtag for private use. This is now supported and can be used to provide a different
translation for specific scenarios, such as a tenant on a multi-tenant application.

The format is: `en-GB-x-Tenant123`, `en-x-Tenant99` etc.

Note the `-x-`, after which you can add four or more alphanumeric characters to specify your custom translation.
There must be an exact match for all subtags for this translation to be returned. If the module can't find a 
translation for the tenant, it will match the remaining subtags according to the algorithm described above.

##### Language Matching Update

The latest refinement to the language matching algoritm:

```csharp
        // Principle Application Language (PAL) Prioritization:
        //   User has selected an explicit language in the webapp e.g. fr-CH (i.e. PAL is set to fr-CH).
        //   Their browser is set to languages en-US, en, zh-Hans.
        //   Therefore, UserLanguages[] equals fr-CH, en-US, zh-Hans.
        //   We don't have a particular message in fr-CH, but have it in fr and fr-CA.
        //   We also have message in en-US and zh-Hans.
        //   Surely, the message from fr or fr-CA is better match than en-US or zh-Hans.
        //   However, without PAL prioritization, en-US is returned and failing that, zh-Hans.
        //   Therefore, for the 1st entry in UserLanguages (i.e. explicit user selection in app)
        //   we try all match grades first. Only if there is no match whatsoever for the PAL
        //   do we move no to the other (browser) languages, where return to prioritizing match grade
        //   i.e. loop through all the languages first at the strictest match grade before loosening 
        //   to the next match grade, and so on.
```

### A reminder about folders in a web application

Your `locale` folder is exposed to HTTP requests as-is, just like a typical log directory, so remember to block all requests
to this folder by adding a `Web.config` file. 

```xml
    <?xml version="1.0"?>
    <configuration>    
        <system.web>
            <httpHandlers>
                <add path="*" verb="*" type="System.Web.HttpNotFoundHandler"/>
            </httpHandlers>
        </system.web>
        <system.webServer>
            <handlers>
                <remove name="BlockViewHandler"/>
                <add name="BlockViewHandler" path="*" verb="*" preCondition="integratedMode" type="System.Web.HttpNotFoundHandler"/>
            </handlers>
        </system.webServer>
    </configuration>
```

### Contributing

There's lot of room for further enhancements and features to this library, and you are encouraged to fork it and
contribute back anything new. Specifically, these would be great places to add more functionality:

* Plurals support.
* Help me fix the bugs! Chances are I don't ship in your language. Fix what hurts. Please?
* Better parsing and handling of PO files for more general purposes / outside editors.
* Input and ideas on a safe universal nugget syntax (see issue #69).
* ASP.NET AJAX support in IIS Classic mode (i.e. non-integrated ASP.NET pipeline) (see issue #109).
