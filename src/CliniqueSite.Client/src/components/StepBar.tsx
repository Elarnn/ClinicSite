interface StepBarProps {
  current: number; // 1-based
  total: number;
}

export function StepBar({ current, total }: StepBarProps) {
  return (
    <div className="step-bar" role="progressbar" aria-valuenow={current} aria-valuemax={total}>
      {Array.from({ length: total }, (_, i) => {
        const step = i + 1;
        const cls =
          step < current ? 'step-dot done' : step === current ? 'step-dot active' : 'step-dot';
        return <div key={i} className={cls} />;
      })}
    </div>
  );
}
