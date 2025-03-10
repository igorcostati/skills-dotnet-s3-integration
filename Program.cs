using System.Threading.Tasks;
using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;

class Program
{

    private static readonly string bucketName = "labs-advc";
    private static readonly RegionEndpoint bucketRegion = RegionEndpoint.USEast1;
    private static readonly IAmazonS3 s3Client = new AmazonS3Client(bucketRegion);

    static async Task Main()
    {
        while (true)
        {
            Console.WriteLine("Escolha uma opção:");
            Console.WriteLine("1 - Upload de arquivo");
            Console.WriteLine("2 - Listar arquivos do bucket");
            Console.WriteLine("3 - Baixar arquivo");
            Console.WriteLine("4 - Apagar arquivo");
            Console.WriteLine("5 - Sair");

            string choice = Console.ReadLine();

            switch (choice)
            {
                case "1":
                    Console.WriteLine("Informe o caminho do arquivo para upload: ");
                    string filePath = Console.ReadLine();
                    await UploadFileAsync(filePath);
                    break;
                case "2":
                    await ListFileAsync();
                    break;
                case "3":
                    Console.WriteLine("Informe o nome do arquivo para download: ");
                    string fileName = Console.ReadLine();
                    string destinationPath = "C:/Users/Admin/Desktop/teste/" + fileName;
                    await DownloadFileAsync(fileName, destinationPath);
                    break;
                case "4":
                    Console.WriteLine("Informe o arquivo para apagar: ");
                    string fileDelete = Console.ReadLine();
                    await DeleteFileAsync(fileDelete);
                    break;
                case "5":
                    return;
                default:
                    Console.WriteLine("Opção inválida");
                    break;
            }
        }
    }

    private static async Task UploadFileAsync(string filePath)
    {
        try
        {
            if (!File.Exists(filePath))
            {
                Console.WriteLine("Arquivo não encontrado");
                return;
            }

            string keyName = Path.GetFileName(filePath);

            var tags = new List<Tag>
            {
                new Tag { Key = "owner", Value = "aws.labs.igor.costa" },
                new Tag { Key = "cost-center", Value = "aws-labs" },
                new Tag { Key = "project", Value = "aws-labs-advc" }
            };

            var putRequest = new PutObjectRequest
            {
                BucketName = bucketName,
                Key = keyName,
                FilePath = filePath,
                ContentType = "application/octet-stream",
                TagSet = tags

            };

            PutObjectResponse response = await s3Client.PutObjectAsync(putRequest);

            Console.WriteLine($"Arquivo {keyName} enviado com sucesso! Status: {response.HttpStatusCode}");

        }
        catch (AmazonS3Exception e)
        {
            Console.WriteLine("Erro do s3: " + e.Message);
        }
        catch (Exception e)
        {
            Console.WriteLine("Erro generico: " + e.Message);
        }
    }

    private static async Task DeleteFileAsync(string keyName)
    {
        try
        {
            var deleteRequest = new DeleteObjectRequest
            {
                BucketName = bucketName,
                Key = keyName
            };

            DeleteObjectResponse response = await s3Client.DeleteObjectAsync(deleteRequest);

            Console.WriteLine($"Arquivo {keyName} apagado com sucesso! Status: {response.HttpStatusCode}");

        }
        catch (AmazonS3Exception e)
        {
            Console.WriteLine("Erro do s3: " + e.Message);
        }
        catch (Exception e)
        {
            Console.WriteLine("Erro generico: " + e.Message);
        }
    }

    private static async Task ListFileAsync()
    {
        try
        {
            var request = new ListObjectsV2Request
            {
                BucketName = bucketName
            };

            var response = await s3Client.ListObjectsV2Async(request);

            Console.WriteLine("Arquivos no bucket:");
            foreach (S3Object entry in response.S3Objects)
            {
                Console.WriteLine($" - {entry.Key} (tamanho = {entry.Size})");
            }
        }
        catch (AmazonS3Exception e)
        {
            Console.WriteLine("Erro do s3: " + e.Message);
        }
        catch (Exception e)
        {
            Console.WriteLine("Erro generico: " + e.Message);
        }
    }

    private static async Task DownloadFileAsync(string fileName, string destinationPath)
    {
        try
        {
            var request = new GetObjectRequest
            {
                BucketName = bucketName,
                Key = fileName
            };

            using (GetObjectResponse response = await s3Client.GetObjectAsync(request))
            using (Stream responseStream = response.ResponseStream)
            using (FileStream fileStream = File.Create(destinationPath))
            {
                await responseStream.CopyToAsync(fileStream);
            }

            Console.WriteLine($"Arquivo {fileName} baixado com sucesso para {destinationPath}");
        }
        catch (AmazonS3Exception e)
        {
            Console.WriteLine("Erro do s3: " + e.Message);
            Console.WriteLine($"Código de Erro: {e.ErrorCode}");
            Console.WriteLine($"Status HTTP: {e.StatusCode}");
            Console.WriteLine($"ID da Solicitação: {e.RequestId}");

        }
        catch (Exception e)
        {
            Console.WriteLine("Erro generico: " + e.Message);
        }
    }
}