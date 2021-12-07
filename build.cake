#addin nuget:?package=Cake.Docker&version=1.0.0
#addin nuget:?package=Cake.Git&version=1.1.0
#addin nuget:?package=Cake.Kubectl&version=1.0.0

//dotnet cake --target Docker && docker images | grep k8sms && docker images | grep k8sms | awk '{print $3}' | xargs docker rmi
var target = Argument("target", "Test");
var configuration = Argument("configuration", "Release");
var dockerUser = Argument("docker-login", "");
var dockerPass = Argument("docker-password", "");

const string solution = "K8sMs";
const string imageAssinaturas = "jedi31/assinaturas";
const string imageCatalogo = "jedi31/catalogo";
const string imageAutorizacao = "jedi31/autorizacao";

const string ingressScript = "https://raw.githubusercontent.com/kubernetes/ingress-nginx/controller-v1.1.0/deploy/static/provider/cloud/deploy.yaml";

Setup(context =>
{
    if (string.IsNullOrWhiteSpace(dockerUser)){
        dockerUser = EnvironmentVariable("DOCKER_USER", string.Empty);
    }

    if (string.IsNullOrWhiteSpace(dockerPass)){
        dockerPass = EnvironmentVariable("DOCKER_PASS", string.Empty);
    }
});

//////////////////////////////////////////////////////////////////////
// FUNCTIONS
//////////////////////////////////////////////////////////////////////

string GetGitSha(){
    return GitLogTip(".").Sha[..7];
}

string[] GetTags(string image){
    //, $"{image}:latest"
    return new string[]{$"{image}:{GetGitSha()}", $"{image}:latest"};
}

void BuildDocker(string image, string path){
    var tags = GetTags(image);
    DockerBuild(new DockerImageBuildSettings{        
        Tag = tags,
        BuildArg = new []{$"APP_VERSION={GetGitSha()}"}
    }, path);

    foreach (var tag in tags)
    {
        DockerPush(tag);
    }
}

string[] GetKubeFiles(){
    return new []{
        ingressScript,
        "./k8s/common/namespaces.yaml",
        "./k8s/assinaturas.yaml",
        "./k8s/catalogo.yaml",
        "./k8s/autorizacao.yaml",
        "./k8s/common/ingress.yaml",
    };
}

//////////////////////////////////////////////////////////////////////
// TASKS
//////////////////////////////////////////////////////////////////////

Task("Clean")
    .WithCriteria(c => HasArgument("rebuild"))
    .Does(() =>
{
    CleanDirectory($"./src/**/bin");
});

Task("Build")
    .IsDependentOn("Clean")
    .Does(() =>
{
    DotNetBuild($"./{solution}.sln", new DotNetCoreBuildSettings
    {
        Configuration = configuration,
    });
});

var dockerAssinaturas = Task("DockerAssinaturas")
    .Does(() =>
{
    BuildDocker(imageAssinaturas, "src/Assinatura");
});

var dockerAutorizacao = Task("DockerAutorizacao")
    .Does(() =>
{
    BuildDocker(imageAutorizacao, "src/Autorizacao");
});

var dockerCatalogo = Task("DockerCatalogo")
    .Does(() =>
{
    BuildDocker(imageCatalogo, "src/Catalogo");
});

var dockerLogin = Task("DockerLogin")
    .Does(() =>
{
    DockerLogin(new DockerRegistryLoginSettings {
        Password = dockerPass,
        Username = dockerUser
    });
});

var docker = Task("Docker")
    // .IsDependentOn("Build")
    .IsDependentOn(dockerLogin)
    .IsDependentOn(dockerAssinaturas)
    .IsDependentOn(dockerAutorizacao)
    .IsDependentOn(dockerCatalogo)
    .Does(() =>
{
});

var test = Task("Test")
    .IsDependentOn("Build")
    .Does(() =>
{
    DotNetTest("./K8sMs.sln", new DotNetCoreTestSettings
    {
        Configuration = configuration,
        NoBuild = true,
    });
});

var k8sIngressUp = Task("kubectl-ingress")
    .Does(() =>
{
    KubectlApply(new KubectlApplySettings {
            Filename = ingressScript
        });
});

var k8sUp = Task("kubectl-up")    
    .Does(() =>
{
    foreach (var file in GetKubeFiles())
    {
        KubectlApply(new KubectlApplySettings {
            Filename = file
        });
    }    
});

var k8sDown = Task("kubectl-down")
    .Does(() =>
{
    foreach (var file in GetKubeFiles().Reverse())
    {
        KubectlDelete(new KubectlDeleteSettings{
            Filename = file,
        });    
    }    
});


//////////////////////////////////////////////////////////////////////
// EXECUTION
//////////////////////////////////////////////////////////////////////

RunTarget(target);