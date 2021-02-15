# laget.Fingerprint.Stores.Mongo
MongoDB store implementation for laget.Fingerprint...

![Nuget](https://img.shields.io/nuget/v/laget.Fingerprint.Stores.Mongo)
![Nuget](https://img.shields.io/nuget/dt/laget.Fingerprint.Stores.Mongo)

## Usage
### Autofac
```c#
public class FingerprintModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.Register<IFingerprintManager<Club>>(c =>
                new FingerprintManager<Club>(new MongoStore<Club>(new MongoUrl(c.Resolve<IConfiguration>().GetConnectionString("MongoConnectionString"))))
            ).SingleInstance();
        }
    }
```
