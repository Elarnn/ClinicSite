import { useState } from 'react';
import type { AppointmentSlotDto, BookingResultDto, DoctorDto, ReserveSlotResultDto, SpecialtyDto } from '../types';
import { StepBar } from '../components/StepBar';
import { SpecialtyStep } from '../components/SpecialtyStep';
import { DoctorStep } from '../components/DoctorStep';
import { SlotStep } from '../components/SlotStep';
import { BookingFormStep } from '../components/BookingFormStep';
import { SuccessStep } from '../components/SuccessStep';

type WizardState =
  | { step: 1 }
  | { step: 2; specialty: SpecialtyDto }
  | { step: 3; specialty: SpecialtyDto; doctor: DoctorDto }
  | { step: 4; specialty: SpecialtyDto; doctor: DoctorDto; slot: AppointmentSlotDto; reservation: ReserveSlotResultDto }
  | { step: 'done'; booking: BookingResultDto };

const STEP_TITLES: Record<number | string, string> = {
  1: 'Choose a Specialty',
  2: 'Choose a Doctor',
  3: 'Choose a Time',
  4: 'Your Details',
};

export function BookingPage() {
  const [state, setState] = useState<WizardState>({ step: 1 });

  const goBack = () => {
    if (state.step === 2) setState({ step: 1 });
    else if (state.step === 3) setState({ step: 2, specialty: state.specialty });
    else if (state.step === 4) setState({ step: 3, specialty: state.specialty, doctor: state.doctor });
  };

  const title = STEP_TITLES[state.step];
  const stepNum = typeof state.step === 'number' ? state.step : 4;
  const isDone = state.step === 'done';
  const showBack = state.step !== 1 && !isDone;

  return (
    <section className="booking-shell">
      <div className="booking-card">
        {/* The success screen brings its own icon + title, so the sticky wizard header is hidden there. */}
        {!isDone && (
          <header className="booking-header">
            <div className="booking-header-row">
              {showBack && (
                <button className="back-btn" onClick={goBack} aria-label="Go back">
                  ‹
                </button>
              )}
              <h1 className="booking-title">{title}</h1>
            </div>
            <StepBar current={stepNum} total={4} />
          </header>
        )}

        <div className="booking-main">
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
        </div>
      </div>
    </section>
  );
}
