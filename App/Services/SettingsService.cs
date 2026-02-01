using System;
using System.Diagnostics;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage;

namespace App.Services
{
    public partial class SettingsService<T> : IDisposable where T : class, new()
    {
        private readonly string _fileName;
        private readonly SemaphoreSlim _lock = new(1, 1);
        private readonly JsonSerializerOptions _jsonOptions;
        private readonly CancellationTokenSource _cts = new();
        
        private T _value;

        public event Action<T>? SettingsChanged;
        
        public T Value => _value;

        public SettingsService(string fileName = "settings.json")
        {
            _fileName = fileName;
            _value = new T();
            _jsonOptions = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNameCaseInsensitive = true
            };

            _value = Load();
        }

        public async Task InitializeAsync(CancellationToken cancellationToken = default)
        {
            cancellationToken = cancellationToken == default ? _cts.Token : cancellationToken;
            await LoadAsync(cancellationToken);
        }

        private T Load()
        {
            try
            {
                var file = ApplicationData.Current.LocalFolder.CreateFileAsync(
                    _fileName,
                    CreationCollisionOption.OpenIfExists).AsTask().GetAwaiter().GetResult();

                var content = FileIO.ReadTextAsync(file).AsTask().GetAwaiter().GetResult();

                if (!string.IsNullOrWhiteSpace(content))
                {
                    var settings = JsonSerializer.Deserialize<T>(content, _jsonOptions);
                    if (settings != null)
                    {
                        Debug.WriteLine("SettingsService: Settings loaded from file");
                        return settings;
                    }
                }

                // Create default settings
                var defaultSettings = new T();
                var json = JsonSerializer.Serialize(defaultSettings, _jsonOptions);
                FileIO.WriteTextAsync(file, json).AsTask().GetAwaiter().GetResult();

                Debug.WriteLine("SettingsService: Created default settings file");
                return defaultSettings;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"SettingsService: Error loading settings: {ex.Message}");
                return new T();
            }
        }

        public async Task<T> LoadAsync(CancellationToken cancellationToken = default)
        {
            cancellationToken = cancellationToken == default ? _cts.Token : cancellationToken;
            
            await _lock.WaitAsync(cancellationToken);
            try
            {
                var file = await ApplicationData.Current.LocalFolder.CreateFileAsync(
                    _fileName, 
                    CreationCollisionOption.OpenIfExists);

                var content = await FileIO.ReadTextAsync(file);
                
                if (!string.IsNullOrWhiteSpace(content))
                {
                    _value = JsonSerializer.Deserialize<T>(content, _jsonOptions) ?? new T();
                }
                else
                {
                    _value = new T();
                    
                    var json = JsonSerializer.Serialize(_value, _jsonOptions);
                    await FileIO.WriteTextAsync(file, json);
                    
                    Debug.WriteLine("SettingsService: Created default settings file");
                }

                return _value;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"SettingsService: Error loading settings: {ex.Message}");
                _value = new T();
                return _value;
            }
            finally
            {
                _lock.Release();
            }
        }

        public async Task SaveAsync(T settings, CancellationToken cancellationToken = default)
        {
            cancellationToken = cancellationToken == default ? _cts.Token : cancellationToken;
            
            Debug.WriteLine("SettingsService: SaveAsync - Waiting for lock...");
            await _lock.WaitAsync(cancellationToken);
            Debug.WriteLine("SettingsService: SaveAsync - Lock acquired");
            
            try
            {
                var file = await ApplicationData.Current.LocalFolder.CreateFileAsync(
                    _fileName,
                    CreationCollisionOption.ReplaceExisting);

                var json = JsonSerializer.Serialize(settings, _jsonOptions);
                await FileIO.WriteTextAsync(file, json);
                Debug.WriteLine($"SettingsService: Settings saved to file");

                _value = settings;
                
                Debug.WriteLine($"SettingsService: Invoking SettingsChanged event, Subscribers: {SettingsChanged?.GetInvocationList().Length ?? 0}");
                SettingsChanged?.Invoke(_value);
                Debug.WriteLine("SettingsService: SettingsChanged event invoked");
            }
            finally
            {
                _lock.Release();
                Debug.WriteLine("SettingsService: SaveAsync - Lock released");
            }
        }

        public async Task UpdateAsync(Action<T> updateAction, CancellationToken cancellationToken = default)
        {
            cancellationToken = cancellationToken == default ? _cts.Token : cancellationToken;
            
            await _lock.WaitAsync(cancellationToken);
            try
            {
                updateAction(_value);
                
                // Save inline to avoid recursive lock
                var file = await ApplicationData.Current.LocalFolder.CreateFileAsync(
                    _fileName,
                    CreationCollisionOption.ReplaceExisting);

                var json = JsonSerializer.Serialize(_value, _jsonOptions);
                await FileIO.WriteTextAsync(file, json);
                
                SettingsChanged?.Invoke(_value);
            }
            finally
            {
                _lock.Release();
            }
        }

        public void Dispose()
        {
            _cts.Cancel();
            _lock.Dispose();
            _cts.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
