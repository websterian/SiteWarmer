# Sitecore Site Warmer
Designed to warm large Commerce sites but can be used to warm any Sitecore site. Can be used by a load balancer or app gateway to ensure a CD is warm before adding it to a pool of CDs. Theoretically, should work with the IIS initialize module but this is untested at the moment.
## Purpose
Warms up entire or portions of a Sitecore site using the sitemap. The sitemap is loaded and each URL is loaded if it meets the criteria setup. Alternatively, the code supports a "soft" warmup whereby the items are simply retrieved. 

Using the sitemap will ensure that all caches are warmed including the HTML cache and if you are using SXA, the SXA caches, per PDP and or Category (PLP). This also includes the Commerce Data provider cache (Redis) and loading entities into the Redis Cache on both the XP and Commerce side.

The warmup process is initiated by a API call on the CD, the first call to the API endpoint starts the warmup process and returns a "Bad request". This is so the app gateway can initiate the warmup then leave the CD out of the pool while it warms. 

The same API endpoint can then be called continuously to see if the CD is warm. 

While the CD is warming a "Bad request" is returned. Once the CD is warm a 200 is returned from the API end point. At this point the app gateway can add the CD back into the pool.

As the API enpoint returns a 200 once its warm and will never start the warm up process again until the CD restarts, there is no chance of a denial of service attack on the public URL. Worst case scenario is that someone warms up your CD for you.

## Features

- API enpoint to start and monitor warmup process, designed to be used from a app gateway or loadbalancer
- Configure by site i.e. multi site support
- Warm by sitemap
- Warm only a certain percentage of the sitemap
- Warm only URLs from the sitemap that contain certain keyswords i.e. can be used for specific categories, items or URLs.
- Specify sitemap download timeout
- Specify sitemap.xml URL
- Configurable verbose logging

## Future features

- Replace dns name with CD machine name

## Installing\Running

### Install

1. Clone or fork repo
2. Build Sitecore.Services.Examples.Warmup.sln
3. Deploy Sitecore.Services.Examples.Warmup.dll and Sitecore.Services.Examples.Warmup.config
4. Deploy items from .item project, this is the items and templates to hold the site warmup settings.

### Configure

1. In sitecore go to /sitecore/system/Settings/Warmup/Warmup Settings
2. Add a new Warm up Site Warmup Setting item per site, there is already a couple examples. You can delete them if you dont need them.
3. There should be a warm up item for each site you want to warm, should look like this...
![Example configuration items](https://github.com/websterian/SiteWarmer/blob/master/Setup.jpg)

#### To run the warmup

1. Go to the URL [MY_SITE_URL]\warmup\probe. For example, https://sx93sc.dev.local/warmup/probe.
