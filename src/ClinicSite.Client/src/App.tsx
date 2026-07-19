import { useState } from 'react';
import './App.css';
import type { AppointmentSlotDto, BookingResultDto, DoctorDto, ReserveSlotResultDto, SpecialtyDto } from './types';
import { StepBar } from './components/StepBar';
import { SpecialtyStep } from './components/SpecialtyStep';
import { DoctorStep } from './components/DoctorStep';
import { SlotStep } from './components/SlotStep';
import { BookingFormStep } from './components/BookingFormStep';
import { SuccessStep } from './components/SuccessStep';

type AppState =
  | { step: 1 }
  | { step: 2; specialty: SpecialtyDto }
  | { step: 3; specialty: SpecialtyDto; doctor: DoctorDto }
  | { step: 4; specialty: SpecialtyDto; doctor: DoctorDto; slot: AppointmentSlotDto; reservation: ReserveSlotResultDto }
  | { step: 'done'; booking: BookingResultDto };

const STEP_TITLES: Record<number | string, string> = {
  1: 'Book Appointment',
  2: 'Choose Doctor',
  3: 'Choose Time',
  4: 'Your Details',
  done: 'Confirmed',
};

export default function App() {
  const [state, setState] = useState<AppState>({ step: 1 });

  const goBack = () => {
    if (state.step === 2) setState({ step: 1 });
    else if (state.step === 3) setState({ step: 2, specialty: state.specialty });
    else if (state.step === 4) setState({ step: 3, specialty: state.specialty, doctor: state.doctor });
  };

  const title = STEP_TITLES[state.step];
  const stepNum = typeof state.step === 'number' ? state.step : 4;
  const showBack = state.step !== 1 && state.step !== 'done';
  const showStepBar = state.step !== 'done';

  return (
    <div className="app">
      <header className="app-header">
        <div className="header-row">
          {showBack && (
            <button className="back-btn" onClick={goBack} aria-label="Go back">
              ‹
            </button>
          )}
          {!showBack && <span className="header-logo">🏥</span>}
          <h1 className="header-title">{title}</h1>
        </div>
        {showStepBar && <StepBar current={stepNum} total={4} />}
      </header>

      <main className="app-main">
        {state.step === 1 && (
          <SpecialtyStep
            onSelect={(specialty) => setState({ step: 2, specialty })}
          />
        )}
        {state.step === 2 && (
          <DoctorStep
            specialty={state.specialty}
            onSelect={(doctor) => setState({ step: 3, specialty: state.specialty, doctor })}
          />
        )}
        {state.step === 3 && (
          <SlotStep
            specialty={state.specialty}
            doctor={state.doctor}
            onSelect={(slot, reservation) =>
              setState({ step: 4, specialty: state.specialty, doctor: state.doctor, slot, reservation })
            }
          />
        )}
        {state.step === 4 && (
          <BookingFormStep
            specialty={state.specialty}
            doctor={state.doctor}
            slot={state.slot}
            reservation={state.reservation}
            onSuccess={(booking) => setState({ step: 'done', booking })}
            onExpired={goBack}
          />
        )}
        {state.step === 'done' && (
          <SuccessStep
            booking={state.booking}
            onBookAgain={() => setState({ step: 1 })}
          />
        )}
      </main>
    </div>
  );
}
