import { useEffect, useState } from 'react';
import StatCard from './components/StatCard';
import './App.css';

interface DashboardStats {
  mensagensPendentes: number;
  mensagensProcessando: number;
  taxaSucesso24h: string;
  taxaFalha24h: string;
  dlqCount: number;
}

function App() {
  const [stats, setStats] = useState<DashboardStats | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [lastUpdated, setLastUpdated] = useState<Date | null>(null);

  const [isSimulating, setIsSimulating] = useState(false);

  const fetchStats = async () => {
    try {
      // Usando fetch nativo
      const response = await fetch('http://localhost:5074/api/v1/Dashboard/stats');
      if (!response.ok) {
        throw new Error('Falha ao buscar dados');
      }
      const data: DashboardStats = await response.json();
      setStats(data);
      setLastUpdated(new Date());
      setError(null);
    } catch (err) {
      console.error(err);
      setError('Não foi possível conectar à WebAPI.');
    }
  };

  const simulateTraffic = async () => {
    setIsSimulating(true);
    const transacoes = Array.from({ length: 50 }).map((_, i) => ({
      documentoFiscal: `NFE-UI-${Date.now()}-${i}`,
      cnpjEmitente: '12345678000199',
      valor: Math.floor(Math.random() * 1000) + 100,
      tipoOperacao: 1,
      payloadOriginal: JSON.stringify({ itens: [{ id: i, valor: 500 }] })
    }));

    try {
      await fetch('http://localhost:5074/api/v1/Transacoes/lote', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
          loteId: crypto.randomUUID(),
          origem: 'Dashboard_Web',
          transacoes
        })
      });
      fetchStats();
    } catch (err) {
      console.error(err);
      alert('Erro ao enviar lote simulado.');
    } finally {
      setIsSimulating(false);
    }
  };

  useEffect(() => {
    // Busca imediata ao carregar
    fetchStats();

    // Configura o polling a cada 5 segundos
    const interval = setInterval(fetchStats, 5000);
    return () => clearInterval(interval);
  }, []);

  return (
    <div className="dashboard">
      <header className="dashboard__header">
        <div className="header__title">
          <h1>Fiscal Middleware</h1>
          <p className="subtitle">Monitoramento em Tempo Real</p>
        </div>
        <div className="header__actions">
          <button 
            className="btn-simulate" 
            onClick={simulateTraffic} 
            disabled={isSimulating}
          >
            {isSimulating ? (
              <>SIMULANDO <div className="spinner-micro"></div></>
            ) : (
              'Simular Lote (50)'
            )}
          </button>
          <div className="header__status">
            <span className={`status-dot ${error ? 'status-dot--error' : 'status-dot--online'}`}></span>
            <span className="status-text">
              {error ? 'Desconectado' : 'Online'}
            </span>
            {lastUpdated && (
              <span className="last-updated">
                Atualizado: {lastUpdated.toLocaleTimeString()}
              </span>
            )}
          </div>
        </div>
      </header>

      <main className="dashboard__content">
        {error && (
          <div className="alert alert--error">
            {error} Certifique-se de que a API está rodando na porta 5074.
          </div>
        )}

        <div className="stats-grid">
          <StatCard 
            title="Pendentes" 
            value={stats?.mensagensPendentes ?? '-'} 
            type="default" 
          />
          <StatCard 
            title="Em Processamento" 
            value={stats?.mensagensProcessando ?? '-'} 
            type="processing" 
          />
          <StatCard 
            title="Taxa de Sucesso (24h)" 
            value={stats?.taxaSucesso24h ?? '-'} 
            type="success" 
          />
          <StatCard 
            title="Taxa de Falha (24h)" 
            value={stats?.taxaFalha24h ?? '-'} 
            type="warning" 
          />
          <StatCard 
            title="Na Fila DLQ" 
            value={stats?.dlqCount ?? '-'} 
            type="error" 
          />
        </div>
      </main>

      <footer className="dashboard__footer">
        <p>&copy; {new Date().getFullYear()} George Dandolini. Todos os direitos reservados.</p>
        <p style={{ marginTop: '0.5rem', opacity: 0.6 }}>Projeto de Redesign UI/UX - Foco em Minimalismo e Microinterações.</p>
      </footer>
    </div>
  );
}

export default App;
