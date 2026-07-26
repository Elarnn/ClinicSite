import { useCallback, useEffect, useMemo, useState } from 'react';
import type { AppointmentStatus, BookingFilter, Dashboard, DoctorBooking, Paged, ScheduleItem } from '../types';
import type { DoctorSession } from '../auth/session';
import { getBookings, getDashboard, getSchedule } from '../api/portal';
import { blockRecurring, blockSlot, unblockRecurring, unblockSlot } from '../api/slots';
import { ApiError } from '../api/http';
import { STATUS_LABELS, fmtDate, fmtTime, statusClass } from '../lib/format';
import { DoctorSummaryCards } from '../components/DoctorSummaryCards';
import { DoctorSchedule } from '../components/DoctorSchedule';
import { DoctorCalendarView } from '../components/DoctorCalendarView';
import { BookingDetailsDrawer } from '../components/BookingDetailsDrawer';
import type { DrawerBooking } from '../components/BookingDetailsDrawer';

type Mode = 'today' | 'week' | 'list';

const STATUS_OPTIONS: AppointmentStatus[] = [
  'Scheduled',
  'CheckedIn',
  'InProgress',
  'Completed',
  'NoShow',
  'Cancelled',
];

interface Props {
  session: DoctorSession;
  onLogout: () => void;
}

function scheduleToDrawer(it: ScheduleItem): DrawerBooking {
  return {
    bookingId: it.bookingId!,
    patientName: it.patientName ?? '',
    patientEmail: it.patientEmail ?? '',
    patientComment: it.patientComment,
    doctorNote: it.doctorNote,
    status: it.status ?? 'Scheduled',
    startTimeUtc: it.startTimeUtc,
    endTimeUtc: it.endTimeUtc,
  };
}

function bookingToDrawer(b: DoctorBooking): DrawerBooking {
  return {
    bookingId: b.bookingId,
    patientName: b.patientName,
    patientEmail: b.patientEmail,
    patientComment: b.patientComment,
    doctorNote: b.doctorNote,
    status: b.status,
    startTimeUtc: b.startTimeUtc,
    endTimeUtc: b.endTimeUtc,
  };
}

function startOfTodayLocal(): Date {
  const now = new Date();
  return new Date(now.getFullYear(), now.getMonth(), now.getDate());
}

export function DoctorDashboardPage({ session, onLogout }: Props) {
  const [mode, setMode] = useState<Mode>('today');
  const [error, setError] = useState<string | null>(null);
  const [selected, setSelected] = useState<DrawerBooking | null>(null);
  const [busySlotId, setBusySlotId] = useState<string | null>(null);

  const [dashboard, setDashboard] = useState<Dashboard | null>(null);
  const [week, setWeek] = useState<ScheduleItem[] | null>(null);
  const [list, setList] = useState<Paged<DoctorBooking> | null>(null);

  const [filter, setFilter] = useState<BookingFilter>({ page: 1, pageSize: 20 });

  const loadToday = useCallback(() => {
    setError(null);
    getDashboard()
      .then(setDashboard)
      .catch((e: unknown) => setError(e instanceof ApiError ? e.message : 'Failed to load dashboard'));
  }, []);

  const loadWeek = useCallback(() => {
    setError(null);
    const from = startOfTodayLocal();
    const to = new Date(from);
    to.setDate(to.getDate() + 7);
    getSchedule(from.toISOString(), to.toISOString())
      .then(setWeek)
      .catch((e: unknown) => setError(e instanceof ApiError ? e.message : 'Failed to load schedule'));
  }, []);

  const loadList = useCallback(() => {
    setError(null);
    getBookings(filter)
      .then(setList)
      .catch((e: unknown) => setError(e instanceof ApiError ? e.message : 'Failed to load bookings'));
  }, [filter]);

  useEffect(() => {
    if (mode === 'today') loadToday();
    else if (mode === 'week') loadWeek();
    else loadList();
  }, [mode, loadToday, loadWeek, loadList]);

  const refresh = useCallback(() => {
    if (mode === 'today') loadToday();
    else if (mode === 'week') loadWeek();
    else loadList();
  }, [mode, loadToday, loadWeek, loadList]);

  function runSlotAction(slotId: string, action: Promise<unknown>) {
    setBusySlotId(slotId);
    action
      .then(() => refresh())
      .catch((e: unknown) => setError(e instanceof ApiError ? e.message : 'Failed to update slot'))
      .finally(() => setBusySlotId(null));
  }

  const highlightBookingId = useMemo(() => {
    if (!dashboard) return null;
    const now = Date.now();
    const upcoming = dashboard.today
      .filter(
        (i) =>
          i.bookingId &&
          new Date(i.startTimeUtc).getTime() >= now &&
          i.status &&
          i.status !== 'Completed' &&
          i.status !== 'NoShow' &&
          i.status !== 'Cancelled',
      )
      .sort((a, b) => new Date(a.startTimeUtc).getTime() - new Date(b.startTimeUtc).getTime());
    return upcoming[0]?.bookingId ?? null;
  }, [dashboard]);

  return (
    <div className="portal">
      <header className="portal-header">
        <div className="brand">
          ClinicSite <span>Doctor Portal</span>
        </div>
        <div className="portal-header-right">
          <span className="portal-user">{session.doctorName}</span>
          <button className="btn-ghost" onClick={onLogout}>
            Sign out
          </button>
        </div>
      </header>

      <main className="portal-main">
        <div className="mode-tabs">
          {(['today', 'week', 'list'] as Mode[]).map((m) => (
            <button key={m} className={`mode-tab${mode === m ? ' active' : ''}`} onClick={() => setMode(m)}>
              {m === 'today' ? 'Today' : m === 'week' ? 'Week' : 'List'}
            </button>
          ))}
        </div>

        {error && <p className="field-error">{error}</p>}

        {mode === 'today' && (
          <div>
            {dashboard === null ? (
              <p className="muted">Loading…</p>
            ) : (
              <>
                <DoctorSummaryCards dashboard={dashboard} />

                <h2 className="page-title">Today's schedule</h2>
                <p className="page-subtitle">
                  “Block daily” makes that time unavailable on every day until you unblock it.
                </p>

                <DoctorSchedule
                  items={dashboard.today}
                  highlightBookingId={highlightBookingId}
                  onOpen={(it) => setSelected(scheduleToDrawer(it))}
                  onBlock={(id) => runSlotAction(id, blockRecurring(id))}
                  onUnblock={(id) => runSlotAction(id, unblockRecurring(id))}
                  busySlotId={busySlotId}
                />
              </>
            )}
          </div>
        )}

        {mode === 'week' && (
          <div>
            <h2 className="page-title">This week</h2>
            <p className="page-subtitle">Block or unblock individual slots here (this specific slot only).</p>
            {week === null ? (
              <p className="muted">Loading…</p>
            ) : (
              <DoctorCalendarView
                items={week}
                onOpen={(it) => setSelected(scheduleToDrawer(it))}
                onBlock={(id) => runSlotAction(id, blockSlot(id))}
                onUnblock={(id) => runSlotAction(id, unblockSlot(id))}
                busySlotId={busySlotId}
              />
            )}
          </div>
        )}

        {mode === 'list' && (
          <ListView
            filter={filter}
            setFilter={setFilter}
            list={list}
            onOpen={(b) => setSelected(bookingToDrawer(b))}
          />
        )}
      </main>

      {selected && (
        <BookingDetailsDrawer booking={selected} onClose={() => setSelected(null)} onChanged={refresh} />
      )}
    </div>
  );
}

interface ListViewProps {
  filter: BookingFilter;
  setFilter: (f: BookingFilter) => void;
  list: Paged<DoctorBooking> | null;
  onOpen: (b: DoctorBooking) => void;
}

function ListView({ filter, setFilter, list, onOpen }: ListViewProps) {
  // Local draft for the free-text / date inputs; applied to the real filter on "Apply".
  const [from, setFrom] = useState(filter.from ? filter.from.slice(0, 10) : '');
  const [to, setTo] = useState(filter.to ? filter.to.slice(0, 10) : '');
  const [status, setStatus] = useState<AppointmentStatus | ''>(filter.status ?? '');
  const [search, setSearch] = useState(filter.search ?? '');

  function apply() {
    setFilter({
      page: 1,
      pageSize: filter.pageSize,
      from: from ? `${from}T00:00:00.000Z` : undefined,
      to: to ? `${to}T23:59:59.999Z` : undefined,
      status: status || undefined,
      search: search.trim() || undefined,
    });
  }

  function goToPage(page: number) {
    setFilter({ ...filter, page });
  }

  return (
    <div>
      <h2 className="page-title">All bookings</h2>

      <div className="filters">
        <div className="slots-field">
          <label className="field-label">From</label>
          <input className="field-input" type="date" value={from} onChange={(e) => setFrom(e.target.value)} />
        </div>
        <div className="slots-field">
          <label className="field-label">To</label>
          <input className="field-input" type="date" value={to} onChange={(e) => setTo(e.target.value)} />
        </div>
        <div className="slots-field">
          <label className="field-label">Status</label>
          <select
            className="field-input"
            value={status}
            onChange={(e) => setStatus(e.target.value as AppointmentStatus | '')}
          >
            <option value="">Any</option>
            {STATUS_OPTIONS.map((s) => (
              <option key={s} value={s}>
                {STATUS_LABELS[s]}
              </option>
            ))}
          </select>
        </div>
        <div className="slots-field grow">
          <label className="field-label">Search (name or email)</label>
          <input className="field-input" value={search} onChange={(e) => setSearch(e.target.value)} />
        </div>
        <button className="btn-primary" onClick={apply}>
          Apply
        </button>
      </div>

      {list === null ? (
        <p className="muted">Loading…</p>
      ) : list.items.length === 0 ? (
        <p className="muted">No bookings match.</p>
      ) : (
        <>
          <div className="table-scroll">
            <table className="data-table">
              <thead>
                <tr>
                  <th>Date</th>
                  <th>Time</th>
                  <th>Patient</th>
                  <th>Email</th>
                  <th>Status</th>
                </tr>
              </thead>
              <tbody>
                {list.items.map((b) => (
                  <tr key={b.bookingId} className="clickable" onClick={() => onOpen(b)}>
                    <td>{fmtDate(b.startTimeUtc)}</td>
                    <td>{fmtTime(b.startTimeUtc)} – {fmtTime(b.endTimeUtc)}</td>
                    <td>{b.patientName}</td>
                    <td>{b.patientEmail}</td>
                    <td><span className={statusClass(b.status)}>{STATUS_LABELS[b.status]}</span></td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>

          <div className="pager">
            <button className="btn-ghost small" disabled={list.page <= 1} onClick={() => goToPage(list.page - 1)}>
              Prev
            </button>
            <span className="muted">
              Page {list.page} of {Math.max(1, list.totalPages)} · {list.totalCount} total
            </span>
            <button
              className="btn-ghost small"
              disabled={list.page >= list.totalPages}
              onClick={() => goToPage(list.page + 1)}
            >
              Next
            </button>
          </div>
        </>
      )}
    </div>
  );
}
