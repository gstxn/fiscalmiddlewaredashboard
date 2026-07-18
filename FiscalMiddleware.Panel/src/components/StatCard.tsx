import React from 'react';
import './StatCard.css';

interface StatCardProps {
  title: string;
  value: string | number;
  type?: 'default' | 'success' | 'error' | 'warning' | 'processing';
  icon?: React.ReactNode;
}

const StatCard: React.FC<StatCardProps> = ({ title, value, type = 'default', icon }) => {
  // Generate random heights for the dummy charts
  const renderSuccessChart = () => (
    <div className="micro-chart">
      {[10, 12, 14, 18, 15, 20, 22, 24, 23, 20].map((h, i) => (
        <div key={i} className="bar-segment" style={{ height: `${h}px`, opacity: 0.3 + (i * 0.05) }} />
      ))}
    </div>
  );

  const renderErrorChart = () => (
    <div className="micro-chart">
      {[4, 2, 8, 20, 15, 6, 12, 4, 18, 24, 8, 4, 10].map((h, i) => (
        <div key={i} className="bar-histogram" style={{ height: `${h}px`, opacity: h > 15 ? 1 : 0.4 }} />
      ))}
    </div>
  );

  const renderProcessingSpinner = () => (
    <div className="spinner-ring">
      <span>{value}</span>
    </div>
  );

  const renderDlqIcon = () => (
    <svg className="dlq-icon" fill="none" stroke="currentColor" viewBox="0 0 24 24" xmlns="http://www.w3.org/2000/svg">
      <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M3 10h10a8 8 0 018 8v2M3 10l6 6m-6-6l6-6" />
    </svg>
  );

  return (
    <div className={`stat-card stat-card--${type}`}>
      <div className="stat-card__header">
        <h3 className="stat-card__title">{title}</h3>
        {icon && <div className="stat-card__icon">{icon}</div>}
      </div>
      <div className="stat-card__content">
        {type === 'processing' ? (
          <>
            <span className="stat-card__value" style={{ opacity: 0 }}>{value}</span> {/* Hidden but keeps height */}
            <div style={{ position: 'absolute', top: 0, left: 0 }}>
               <span className="stat-card__value">{value}</span>
            </div>
            {renderProcessingSpinner()}
          </>
        ) : (
          <span className="stat-card__value">{value}</span>
        )}

        {type === 'success' && renderSuccessChart()}
        {type === 'warning' && renderErrorChart()}
        {type === 'error' && renderDlqIcon()}
      </div>
    </div>
  );
};

export default StatCard;
