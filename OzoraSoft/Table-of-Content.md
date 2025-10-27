<!-- Logo -->
![OzoraSoft](https://cdn1.site-media.eu/images/0/19595122/ozorasoft_logo-100-e5G4JCZXTmujqxhPEmoCcA.png)

# Solution Folders
- 0-SharedLibraries
   - OzoraSoft.Library.Security
   - Orestes.SharedLibrary
- 1-DataSources
   - OzoraSoft.Library.DataSources
- 2-MicroServices
   - OzoraSoft.API.Services
   - OzoraSoft.API.Utils
- 3-WebApps
   - OzoraSoft.AppHost
   - OzoraSoft.ServiceDefaults
   - OzoraSoft.Web_
- 4-Desktops
   - OzoraSoft.Console
   - OzoraSoft.Desktop
- 5-UnitTests
   - OzoraSoft.Tests
---
<!-- Project Plan-->
# Solution Planning
- [x] Solution Structure
- [x] API.Utils
- [ ] API.Services
- [ ] Unit Tests
- [ ] Web App
- [ ] Desktop App
---
<!-- Blank -->

<!-- Blank -->
<!-- Solution Components -->
# Solution Components
::: mermaid

flowchart LR
    subgraph A["App Host"]
    direction LR
        WebApp --> MicroServices  
    end
    subgraph B["Desktop Apps"]
    direction RL
        DesktopApp
        ConsoleApp
        UnitTests
    end
    subgraph C["Shared Libraries"]
    direction LR
        OzoraSoft.SharedLibrary
        Orestes.SharedLibrary
    end
    subgraph D["Data Access"]
    direction LR
        MicroServices --> DataSources     
        DataSources --> Database@{ shape: lin-cyl, label: "MySQL DB" }   
    end
    B --> MicroServices 
    C --> A & B
:::


