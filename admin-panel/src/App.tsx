import { useEffect, useMemo, useState } from 'react';
import type { FormEvent, ReactNode } from 'react';
import { adminService } from './services/adminService';
import type {
    ApartmentDto,
    AvailableRoomDto,
    BuildingDto,
    BuildingResidentDto,
    ContractorType,
    CreatedUserDto,
    FloorDto,
    MaintenanceRequestDto,
    RoomTemplate,
    RoomType,
    StaffDto,
    StaffRole,
} from './services/adminService';
import { API_BASE_URL } from './services/api';

type Tab = 'setup' | 'people' | 'maintenance' | 'notifications';

const defaultRooms: RoomTemplate[] = [
    { number: 'A', type: 'Studio', maxOccupancy: 1, squareMeters: 22, monthlyRent: 850 },
];

const staffRoles: StaffRole[] = ['BuildingAdmin', 'Reception', 'Cleaner', 'Contractor', 'Security'];
const contractorTypes: ContractorType[] = [
    'Plumbing',
    'Electric',
    'Furniture',
    'Appliance',
    'Hvac',
    'WindowsAndDoors',
    'Internet',
    'Other',
];
const roomTypes: RoomType[] = ['Studio', 'Single', 'Double', 'Shared'];

function App() {
    const [token, setToken] = useState(() => localStorage.getItem('admin_token') || '');
    const [mustChangePassword, setMustChangePassword] = useState(false);
    const [adminEmail, setAdminEmail] = useState('maria.admin@fizz.test');
    const [adminPassword, setAdminPassword] = useState('fizz123!');
    const [oldPassword, setOldPassword] = useState('');
    const [newPassword, setNewPassword] = useState('');

    const [tab, setTab] = useState<Tab>('setup');
    const [busy, setBusy] = useState(false);
    const [message, setMessage] = useState<string | null>(null);

    const [buildings, setBuildings] = useState<BuildingDto[]>([]);
    const [selectedBuildingId, setSelectedBuildingId] = useState(() => localStorage.getItem('admin_building_id') || '');
    const [floors, setFloors] = useState<FloorDto[]>([]);
    const [apartments, setApartments] = useState<ApartmentDto[]>([]);
    const [availableRooms, setAvailableRooms] = useState<AvailableRoomDto[]>([]);
    const [residents, setResidents] = useState<BuildingResidentDto[]>([]);
    const [staff, setStaff] = useState<StaffDto[]>([]);
    const [maintenance, setMaintenance] = useState<MaintenanceRequestDto[]>([]);

    const [apartmentFloorId, setApartmentFloorId] = useState('');
    const [unitNumber, setUnitNumber] = useState('');
    const [apartmentName, setApartmentName] = useState('');
    const [rooms, setRooms] = useState<RoomTemplate[]>(defaultRooms);

    const [tenantName, setTenantName] = useState('');
    const [tenantEmail, setTenantEmail] = useState('');
    const [tenantRoomId, setTenantRoomId] = useState('');
    const [staffName, setStaffName] = useState('');
    const [staffEmail, setStaffEmail] = useState('');
    const [staffRole, setStaffRole] = useState<StaffRole>('Contractor');
    const [staffSpecialization, setStaffSpecialization] = useState<ContractorType>('Plumbing');
    const [lastCreatedUser, setLastCreatedUser] = useState<CreatedUserDto | null>(null);

    const [noticeAudience, setNoticeAudience] = useState<'building' | 'personal'>('building');
    const [noticeTargetUserId, setNoticeTargetUserId] = useState('');
    const [noticeTitle, setNoticeTitle] = useState('');
    const [noticeBody, setNoticeBody] = useState('');
    const [noticeImportant, setNoticeImportant] = useState(true);

    const selectedBuilding = useMemo(
        () => buildings.find((building) => building.id === selectedBuildingId) || null,
        [buildings, selectedBuildingId]
    );

    const assignees = useMemo(
        () => staff.filter((item) => item.isActive && ['Contractor', 'Cleaner'].includes(item.role)),
        [staff]
    );

    useEffect(() => {
        if (token && !mustChangePassword) {
            void loadBuildings();
        }
    }, [token, mustChangePassword]);

    useEffect(() => {
        if (selectedBuildingId) {
            localStorage.setItem('admin_building_id', selectedBuildingId);
            void loadBuildingData(selectedBuildingId);
        }
    }, [selectedBuildingId]);

    useEffect(() => {
        if (!apartmentFloorId && floors.length > 0) {
            setApartmentFloorId(floors[0].id);
        }
    }, [floors, apartmentFloorId]);

    useEffect(() => {
        if (!tenantRoomId && availableRooms.length > 0) {
            setTenantRoomId(availableRooms[0].roomId);
        }
    }, [availableRooms, tenantRoomId]);

    useEffect(() => {
        if (!noticeTargetUserId && residents.length > 0) {
            setNoticeTargetUserId(residents[0].userId);
        }
    }, [residents, noticeTargetUserId]);

    async function run(action: () => Promise<void>, success?: string) {
        setBusy(true);
        setMessage(null);
        try {
            await action();
            if (success) setMessage(success);
        } catch (error) {
            setMessage(error instanceof Error ? error.message : 'Ошибка запроса');
        } finally {
            setBusy(false);
        }
    }

    async function loadBuildings() {
        await run(async () => {
            const list = await adminService.getBuildings();
            setBuildings(list);
            const nextBuildingId = selectedBuildingId && list.some((b) => b.id === selectedBuildingId)
                ? selectedBuildingId
                : list[0]?.id || '';
            setSelectedBuildingId(nextBuildingId);
        });
    }

    async function loadBuildingData(buildingId: string) {
        await run(async () => {
            const [floorList, apartmentList, roomList, residentList, staffList, maintenanceList] = await Promise.all([
                adminService.getFloors(buildingId),
                adminService.getApartments(buildingId),
                adminService.getAvailableRooms(buildingId),
                adminService.getResidents(buildingId),
                adminService.getStaff(buildingId),
                adminService.getMaintenance(buildingId),
            ]);
            setFloors(floorList);
            setApartments(apartmentList);
            setAvailableRooms(roomList);
            setResidents(residentList);
            setStaff(staffList);
            setMaintenance(maintenanceList);
        });
    }

    async function handleLogin(event: FormEvent) {
        event.preventDefault();
        await run(async () => {
            const result = await adminService.login(adminEmail.trim(), adminPassword);
            localStorage.setItem('admin_token', result.token);
            setToken(result.token);
            setMustChangePassword(result.mustChangePassword);
            setOldPassword(adminPassword);
        });
    }

    async function handleChangePassword(event: FormEvent) {
        event.preventDefault();
        await run(async () => {
            const result = await adminService.changePassword(oldPassword, newPassword);
            localStorage.setItem('admin_token', result.token);
            setToken(result.token);
            setMustChangePassword(false);
            setOldPassword('');
            setNewPassword('');
        }, 'Пароль обновлён');
    }

    function logout() {
        localStorage.removeItem('admin_token');
        setToken('');
        setBuildings([]);
        setSelectedBuildingId('');
    }

    function applyRoomTemplate(template: 'studio' | 'twin' | 'four') {
        if (template === 'studio') {
            setRooms([{ number: 'A', type: 'Studio', maxOccupancy: 1, squareMeters: 22, monthlyRent: 850 }]);
        }
        if (template === 'twin') {
            setRooms([{ number: 'A', type: 'Double', maxOccupancy: 2, squareMeters: 26, monthlyRent: 550 }]);
        }
        if (template === 'four') {
            setRooms(['A', 'B', 'C', 'D'].map((number) => ({
                number,
                type: 'Single',
                maxOccupancy: 1,
                squareMeters: 14,
                monthlyRent: 620,
            })));
        }
    }

    async function handleCreateApartment(event: FormEvent) {
        event.preventDefault();
        if (!selectedBuildingId || !apartmentFloorId) return;

        await run(async () => {
            await adminService.createApartment(selectedBuildingId, apartmentFloorId, unitNumber, apartmentName, rooms);
            setUnitNumber('');
            setApartmentName('');
            setRooms(defaultRooms);
            await loadBuildingData(selectedBuildingId);
        }, 'Юнит создан');
    }

    async function handleCreateTenant(event: FormEvent) {
        event.preventDefault();
        if (!selectedBuildingId || !tenantRoomId) return;

        await run(async () => {
            const created = await adminService.createTenant(selectedBuildingId, tenantEmail, tenantName, tenantRoomId);
            setLastCreatedUser(created);
            setTenantEmail('');
            setTenantName('');
            await loadBuildingData(selectedBuildingId);
        }, 'Resident account создан');
    }

    async function handleCreateStaff(event: FormEvent) {
        event.preventDefault();
        if (!selectedBuildingId) return;

        await run(async () => {
            const created = await adminService.createStaff(
                selectedBuildingId,
                staffEmail,
                staffName,
                staffRole,
                staffRole === 'Contractor' ? staffSpecialization : null
            );
            setLastCreatedUser(created);
            setStaffEmail('');
            setStaffName('');
            await loadBuildingData(selectedBuildingId);
        }, 'Staff account создан');
    }

    async function handleAcknowledge(id: string) {
        if (!selectedBuildingId) return;
        await run(async () => {
            await adminService.acknowledgeMaintenance(id);
            await loadBuildingData(selectedBuildingId);
        }, 'Заявка принята');
    }

    async function handleAssign(id: string, staffAssignmentId: string) {
        if (!selectedBuildingId || !staffAssignmentId) return;
        await run(async () => {
            await adminService.assignMaintenance(id, staffAssignmentId);
            await loadBuildingData(selectedBuildingId);
        }, 'Исполнитель назначен');
    }

    async function handleSendNotice(event: FormEvent) {
        event.preventDefault();
        if (!selectedBuildingId) return;

        await run(async () => {
            if (noticeAudience === 'personal') {
                await adminService.sendPersonalNotification(
                    selectedBuildingId,
                    noticeTargetUserId,
                    noticeTitle,
                    noticeBody,
                    noticeImportant
                );
            } else {
                await adminService.sendBuildingNotification(
                    selectedBuildingId,
                    noticeTitle,
                    noticeBody,
                    noticeImportant
                );
            }
            setNoticeTitle('');
            setNoticeBody('');
        }, 'Уведомление отправлено');
    }

    if (!token) {
        return (
            <AuthShell title="Backoffice login" message={message} busy={busy}>
                <form className="auth-form" onSubmit={handleLogin}>
                    <label>Email</label>
                    <input value={adminEmail} onChange={(e) => setAdminEmail(e.target.value)} type="email" />
                    <label>Password</label>
                    <input value={adminPassword} onChange={(e) => setAdminPassword(e.target.value)} type="password" />
                    <button className="primary-button" disabled={busy}>
                        <i className="bi bi-box-arrow-in-right" />
                        Sign in
                    </button>
                    <p className="helper-text">API: {API_BASE_URL}</p>
                </form>
            </AuthShell>
        );
    }

    if (mustChangePassword) {
        return (
            <AuthShell title="Change temporary password" message={message} busy={busy}>
                <form className="auth-form" onSubmit={handleChangePassword}>
                    <label>Temporary password</label>
                    <input value={oldPassword} onChange={(e) => setOldPassword(e.target.value)} type="password" />
                    <label>New password</label>
                    <input value={newPassword} onChange={(e) => setNewPassword(e.target.value)} type="password" minLength={8} />
                    <button className="primary-button" disabled={busy}>
                        <i className="bi bi-key-fill" />
                        Save password
                    </button>
                </form>
            </AuthShell>
        );
    }

    return (
        <div className="app-shell">
            <aside className="sidebar">
                <div className="brand">
                    <div className="brand-mark">C</div>
                    <div>
                        <strong>Co-Living OS</strong>
                        <span>Internal backoffice</span>
                    </div>
                </div>

                <label>Building</label>
                <select value={selectedBuildingId} onChange={(e) => setSelectedBuildingId(e.target.value)}>
                    {buildings.map((building) => (
                        <option key={building.id} value={building.id}>
                            {building.name}
                        </option>
                    ))}
                </select>

                <nav className="side-nav">
                    <TabButton tab="setup" active={tab} onClick={setTab} icon="bi-building">Rooms</TabButton>
                    <TabButton tab="people" active={tab} onClick={setTab} icon="bi-people-fill">Accounts</TabButton>
                    <TabButton tab="maintenance" active={tab} onClick={setTab} icon="bi-tools">Problems</TabButton>
                    <TabButton tab="notifications" active={tab} onClick={setTab} icon="bi-bell-fill">Notices</TabButton>
                </nav>

                <button className="secondary-button" onClick={() => selectedBuildingId && loadBuildingData(selectedBuildingId)} disabled={busy}>
                    <i className="bi bi-arrow-clockwise" />
                    Refresh
                </button>
                <button className="danger-button" onClick={logout}>
                    <i className="bi bi-box-arrow-left" />
                    Logout
                </button>
            </aside>

            <main className="workspace">
                <header className="topbar">
                    <div>
                        <h1>{selectedBuilding?.name || 'Backoffice'}</h1>
                        <p>{selectedBuilding ? `${selectedBuilding.city}, ${selectedBuilding.country}` : 'No building selected'}</p>
                    </div>
                    {busy && <span className="status-pill">Working...</span>}
                    {message && <span className="status-pill">{message}</span>}
                </header>

                {tab === 'setup' && (
                    <section className="grid-two">
                        <form className="panel" onSubmit={handleCreateApartment}>
                            <PanelTitle icon="bi-door-open-fill" title="Create unit and rooms" />
                            <div className="form-row">
                                <label>Floor</label>
                                <select value={apartmentFloorId} onChange={(e) => setApartmentFloorId(e.target.value)}>
                                    {floors.map((floor) => (
                                        <option key={floor.id} value={floor.id}>
                                            {floor.name || `Floor ${floor.number}`}
                                        </option>
                                    ))}
                                </select>
                            </div>
                            <div className="form-grid">
                                <div>
                                    <label>Unit number</label>
                                    <input value={unitNumber} onChange={(e) => setUnitNumber(e.target.value)} placeholder="0205" required />
                                </div>
                                <div>
                                    <label>Name</label>
                                    <input value={apartmentName} onChange={(e) => setApartmentName(e.target.value)} placeholder="Shared Flat 0205" />
                                </div>
                            </div>
                            <div className="segmented">
                                <button type="button" onClick={() => applyRoomTemplate('studio')}>Studio</button>
                                <button type="button" onClick={() => applyRoomTemplate('twin')}>Twin</button>
                                <button type="button" onClick={() => applyRoomTemplate('four')}>4 room</button>
                            </div>
                            <div className="room-editor">
                                {rooms.map((room, index) => (
                                    <div className="room-row" key={`${room.number}-${index}`}>
                                        <input
                                            value={room.number}
                                            onChange={(e) => updateRoom(index, { number: e.target.value })}
                                            placeholder="A"
                                        />
                                        <select value={room.type} onChange={(e) => updateRoom(index, { type: e.target.value as RoomType })}>
                                            {roomTypes.map((type) => <option key={type}>{type}</option>)}
                                        </select>
                                        <input
                                            value={room.maxOccupancy}
                                            type="number"
                                            min={1}
                                            onChange={(e) => updateRoom(index, { maxOccupancy: Number(e.target.value) })}
                                        />
                                        <button type="button" className="icon-button" onClick={() => setRooms((prev) => prev.filter((_, i) => i !== index))}>
                                            <i className="bi bi-trash3" />
                                        </button>
                                    </div>
                                ))}
                            </div>
                            <button type="button" className="secondary-button" onClick={() => setRooms((prev) => [...prev, { number: '', type: 'Single', maxOccupancy: 1 }])}>
                                <i className="bi bi-plus-lg" />
                                Add room
                            </button>
                            <button className="primary-button" disabled={busy}>Create unit</button>
                        </form>

                        <div className="panel">
                            <PanelTitle icon="bi-layout-three-columns" title="Current rooms" />
                            <div className="list">
                                {apartments.map((apartment) => (
                                    <div className="list-item" key={apartment.apartmentId}>
                                        <div>
                                            <strong>{apartment.unitNumber || apartment.name}</strong>
                                            <span>Floor {apartment.floorNumber}</span>
                                        </div>
                                        <div className="pill-wrap">
                                            {apartment.rooms.map((room) => (
                                                <span className="mini-pill" key={room.roomId}>
                                                    {room.number} {room.currentOccupants}/{room.maxOccupancy}
                                                </span>
                                            ))}
                                        </div>
                                    </div>
                                ))}
                            </div>
                        </div>
                    </section>
                )}

                {tab === 'people' && (
                    <section className="grid-two">
                        <form className="panel" onSubmit={handleCreateTenant}>
                            <PanelTitle icon="bi-person-plus-fill" title="Create resident account" />
                            <label>Name</label>
                            <input value={tenantName} onChange={(e) => setTenantName(e.target.value)} required />
                            <label>Email</label>
                            <input value={tenantEmail} onChange={(e) => setTenantEmail(e.target.value)} type="email" required />
                            <label>Room</label>
                            <select value={tenantRoomId} onChange={(e) => setTenantRoomId(e.target.value)}>
                                {availableRooms.map((room) => (
                                    <option key={room.roomId} value={room.roomId}>
                                        {room.unitNumber || room.apartmentName} / {room.roomNumber} ({room.currentOccupants}/{room.maxOccupancy})
                                    </option>
                                ))}
                            </select>
                            <button className="primary-button" disabled={busy}>Create resident</button>
                        </form>

                        <form className="panel" onSubmit={handleCreateStaff}>
                            <PanelTitle icon="bi-person-workspace" title="Create staff account" />
                            <label>Name</label>
                            <input value={staffName} onChange={(e) => setStaffName(e.target.value)} required />
                            <label>Email</label>
                            <input value={staffEmail} onChange={(e) => setStaffEmail(e.target.value)} type="email" required />
                            <div className="form-grid">
                                <div>
                                    <label>Role</label>
                                    <select value={staffRole} onChange={(e) => setStaffRole(e.target.value as StaffRole)}>
                                        {staffRoles.map((role) => <option key={role}>{role}</option>)}
                                    </select>
                                </div>
                                <div>
                                    <label>Specialization</label>
                                    <select
                                        value={staffSpecialization}
                                        onChange={(e) => setStaffSpecialization(e.target.value as ContractorType)}
                                        disabled={staffRole !== 'Contractor'}
                                    >
                                        {contractorTypes.map((type) => <option key={type}>{type}</option>)}
                                    </select>
                                </div>
                            </div>
                            <button className="primary-button" disabled={busy}>Create staff</button>
                        </form>

                        {lastCreatedUser && (
                            <div className="panel attention-panel">
                                <PanelTitle icon="bi-key-fill" title="Temporary credentials" />
                                <p>Show this once to the user, then ask them to change the password on first login.</p>
                                <code>{lastCreatedUser.email}</code>
                                <code>{lastCreatedUser.tempPassword}</code>
                            </div>
                        )}

                        <div className="panel">
                            <PanelTitle icon="bi-people-fill" title="Residents" />
                            <div className="list compact">
                                {residents.map((resident) => (
                                    <div className="list-item" key={`${resident.userId}-${resident.roomId}`}>
                                        <div>
                                            <strong>{resident.name}</strong>
                                            <span>{resident.email}</span>
                                        </div>
                                        <span className="mini-pill">{resident.unitNumber || resident.apartmentName} / {resident.roomNumber || '-'}</span>
                                    </div>
                                ))}
                            </div>
                        </div>
                    </section>
                )}

                {tab === 'maintenance' && (
                    <section className="panel">
                        <PanelTitle icon="bi-tools" title="Resident problems" />
                        <div className="list">
                            {maintenance.map((request) => (
                                <div className="maintenance-item" key={request.id}>
                                    <div>
                                        <div className="item-heading">
                                            <strong>{request.title}</strong>
                                            <span className={`status status-${request.status.toLowerCase()}`}>{request.status}</span>
                                        </div>
                                        <p>{request.description}</p>
                                        <span>{request.reporterName} · {[request.unitNumber, request.roomNumber].filter(Boolean).join(' / ') || 'building area'}</span>
                                    </div>
                                    <div className="item-actions">
                                        {request.status === 'Reported' && (
                                            <button className="secondary-button" onClick={() => handleAcknowledge(request.id)} disabled={busy}>
                                                Accept
                                            </button>
                                        )}
                                        {['Reported', 'Acknowledged', 'Assigned'].includes(request.status) && (
                                            <select onChange={(e) => e.target.value && handleAssign(request.id, e.target.value)} defaultValue="">
                                                <option value="">Assign...</option>
                                                {assignees.map((person) => (
                                                    <option key={person.staffAssignmentId} value={person.staffAssignmentId}>
                                                        {person.name} · {person.specialization || person.role}
                                                    </option>
                                                ))}
                                            </select>
                                        )}
                                    </div>
                                </div>
                            ))}
                            {maintenance.length === 0 && <div className="empty-state">No maintenance requests yet.</div>}
                        </div>
                    </section>
                )}

                {tab === 'notifications' && (
                    <section className="grid-two">
                        <form className="panel" onSubmit={handleSendNotice}>
                            <PanelTitle icon="bi-megaphone-fill" title="Send notification" />
                            <label>Audience</label>
                            <div className="segmented">
                                <button type="button" className={noticeAudience === 'building' ? 'selected' : ''} onClick={() => setNoticeAudience('building')}>
                                    Whole building
                                </button>
                                <button type="button" className={noticeAudience === 'personal' ? 'selected' : ''} onClick={() => setNoticeAudience('personal')}>
                                    Personal
                                </button>
                            </div>
                            {noticeAudience === 'personal' && (
                                <>
                                    <label>Resident</label>
                                    <select value={noticeTargetUserId} onChange={(e) => setNoticeTargetUserId(e.target.value)}>
                                        {residents.map((resident) => (
                                            <option key={resident.userId} value={resident.userId}>
                                                {resident.name} · {resident.unitNumber || resident.apartmentName}
                                            </option>
                                        ))}
                                    </select>
                                </>
                            )}
                            <label>Title</label>
                            <input value={noticeTitle} onChange={(e) => setNoticeTitle(e.target.value)} maxLength={160} required />
                            <label>Message</label>
                            <textarea value={noticeBody} onChange={(e) => setNoticeBody(e.target.value)} rows={6} maxLength={2000} required />
                            <label className="check-row">
                                <input type="checkbox" checked={noticeImportant} onChange={(e) => setNoticeImportant(e.target.checked)} />
                                Mark as important
                            </label>
                            <button className="primary-button" disabled={busy}>Send notification</button>
                        </form>

                        <div className="panel">
                            <PanelTitle icon="bi-info-circle-fill" title="MVP behavior" />
                            <p className="body-copy">
                                Building-wide notifications are saved as separate resident notifications. The iOS app can show important unread items on Home and all items in Settings or Notifications later.
                            </p>
                        </div>
                    </section>
                )}
            </main>
        </div>
    );

    function updateRoom(index: number, patch: Partial<RoomTemplate>) {
        setRooms((prev) => prev.map((room, i) => i === index ? { ...room, ...patch } : room));
    }
}

function AuthShell({ title, message, busy, children }: { title: string; message: string | null; busy: boolean; children: ReactNode }) {
    return (
        <main className="auth-shell">
            <section className="auth-card">
                <div className="brand compact-brand">
                    <div className="brand-mark">C</div>
                    <div>
                        <strong>Co-Living OS</strong>
                        <span>{title}</span>
                    </div>
                </div>
                {children}
                {busy && <p className="helper-text">Working...</p>}
                {message && <p className="form-message">{message}</p>}
            </section>
        </main>
    );
}

function TabButton({ tab, active, icon, children, onClick }: {
    tab: Tab;
    active: Tab;
    icon: string;
    children: ReactNode;
    onClick: (tab: Tab) => void;
}) {
    return (
        <button className={active === tab ? 'active' : ''} onClick={() => onClick(tab)}>
            <i className={`bi ${icon}`} />
            {children}
        </button>
    );
}

function PanelTitle({ icon, title }: { icon: string; title: string }) {
    return (
        <div className="panel-title">
            <i className={`bi ${icon}`} />
            <h2>{title}</h2>
        </div>
    );
}

export default App;
