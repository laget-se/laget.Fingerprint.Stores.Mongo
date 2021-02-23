# laget.Fingerprint.Stores.Mongo
MongoDB store implementation for laget.Fingerprint...

![Nuget](https://img.shields.io/nuget/v/laget.Fingerprint.Stores.Mongo)
![Nuget](https://img.shields.io/nuget/dt/laget.Fingerprint.Stores.Mongo)

## Configuration
> This example is shown using Autofac since this is the go-to IoC for us.
```c#
public class FingerprintModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        builder.Register<IFingerprintManager<Fingerprint, User>>(c =>
            new FingerprintManager<Fingerprint, User>(new MongoStore<Fingerprint, User>(new MongoUrl(c.Resolve<IConfiguration>().GetConnectionString("MongoConnectionString"))))
        ).SingleInstance();
    }
}
```
