import { StatusBar } from 'expo-status-bar';
import { useEffect, useMemo, useState } from 'react';
import {
  ActivityIndicator,
  KeyboardAvoidingView,
  Linking,
  Platform,
  Pressable,
  SafeAreaView,
  ScrollView,
  StyleSheet,
  Text,
  TextInput,
  View,
} from 'react-native';
import { api } from './src/api/client';
import {
  CreateMaintenanceRequest,
  MaintenanceCategory,
  MaintenancePriority,
  MaintenanceRequestDto,
  MyApartmentContextDto,
  ResidentNotificationDto,
} from './src/api/types';
import { PRIVACY_POLICY_URL, SUPPORT_EMAIL } from './src/config';
import { sessionStorage } from './src/storage/sessionStorage';

type Screen = 'home' | 'maintenance' | 'settings';

type LocationOption = {
  key: string;
  label: string;
  payload: Pick<CreateMaintenanceRequest, 'roomId' | 'apartmentId' | 'buildingId'>;
};

const categories: MaintenanceCategory[] = [
  'Plumbing',
  'Electric',
  'Furniture',
  'Appliance',
  'Hvac',
  'WindowsAndDoors',
  'Cleaning',
  'Internet',
  'Other',
];

const priorities: MaintenancePriority[] = ['Low', 'Normal', 'High', 'Urgent'];

export default function App() {
  const [booting, setBooting] = useState(true);
  const [busy, setBusy] = useState(false);
  const [token, setToken] = useState<string | null>(null);
  const [mustChangePassword, setMustChangePassword] = useState(false);
  const [screen, setScreen] = useState<Screen>('home');
  const [message, setMessage] = useState<string | null>(null);

  const [context, setContext] = useState<MyApartmentContextDto | null>(null);
  const [notifications, setNotifications] = useState<ResidentNotificationDto[]>([]);
  const [maintenance, setMaintenance] = useState<MaintenanceRequestDto[]>([]);

  useEffect(() => {
    async function boot() {
      const storedToken = await sessionStorage.getToken();
      if (storedToken) {
        setToken(storedToken);
        await loadResidentData(storedToken);
      }
      setBooting(false);
    }

    void boot();
  }, []);

  const importantNotices = useMemo(
    () => notifications.filter((notice) => notice.isImportant && !notice.isRead).slice(0, 3),
    [notifications]
  );

  const locationOptions = useMemo(() => buildLocationOptions(context), [context]);

  async function run(action: () => Promise<void>, success?: string) {
    setBusy(true);
    setMessage(null);
    try {
      await action();
      if (success) setMessage(success);
    } catch (error) {
      setMessage(error instanceof Error ? error.message : 'Something went wrong');
    } finally {
      setBusy(false);
    }
  }

  async function loadResidentData(authToken = token) {
    if (!authToken) return;
    await run(async () => {
      const [nextContext, nextNotices, nextMaintenance] = await Promise.all([
        api.getMyContext(authToken),
        api.getNotifications(authToken),
        api.getMaintenance(authToken),
      ]);
      setContext(nextContext);
      setNotifications(nextNotices);
      setMaintenance(nextMaintenance);
    });
  }

  async function handleLogin(email: string, password: string) {
    await run(async () => {
      const result = await api.login(email, password);
      setToken(result.token);
      setMustChangePassword(result.mustChangePassword);
      if (!result.mustChangePassword) {
        await sessionStorage.setToken(result.token);
        await loadResidentData(result.token);
      }
    });
  }

  async function handlePasswordChange(oldPassword: string, newPassword: string) {
    if (!token) return;
    await run(async () => {
      const result = await api.changePassword(token, oldPassword, newPassword);
      await sessionStorage.setToken(result.token);
      setToken(result.token);
      setMustChangePassword(false);
      await loadResidentData(result.token);
    }, 'Password changed');
  }

  async function handleLogout() {
    await sessionStorage.clearToken();
    setToken(null);
    setContext(null);
    setNotifications([]);
    setMaintenance([]);
    setScreen('home');
  }

  async function refresh() {
    await loadResidentData();
  }

  if (booting) {
    return (
      <SafeAreaView style={styles.centered}>
        <ActivityIndicator />
        <Text style={styles.muted}>Loading session</Text>
        <StatusBar style="dark" />
      </SafeAreaView>
    );
  }

  if (!token) {
    return (
      <LoginScreen
        busy={busy}
        message={message}
        onLogin={handleLogin}
      />
    );
  }

  if (mustChangePassword) {
    return (
      <ChangePasswordScreen
        busy={busy}
        message={message}
        onChangePassword={handlePasswordChange}
      />
    );
  }

  return (
    <SafeAreaView style={styles.shell}>
      <StatusBar style="dark" />
      <View style={styles.header}>
        <View>
          <Text style={styles.appName}>Co-Living OS</Text>
          <Text style={styles.headerSub}>{context?.buildingName || 'Resident app'}</Text>
        </View>
        <Pressable style={styles.iconButton} onPress={refresh} disabled={busy}>
          <Text style={styles.iconButtonText}>{busy ? '...' : 'Refresh'}</Text>
        </Pressable>
      </View>

      {message && <Text style={styles.message}>{message}</Text>}

      {screen === 'home' && (
        <HomeScreen
          context={context}
          importantNotices={importantNotices}
          onReadNotice={(id) => token && run(async () => {
            await api.markNotificationRead(token, id);
            await loadResidentData(token);
          })}
        />
      )}

      {screen === 'maintenance' && (
        <MaintenanceScreen
          token={token}
          context={context}
          locationOptions={locationOptions}
          items={maintenance}
          busy={busy}
          onChanged={() => loadResidentData(token)}
          run={run}
        />
      )}

      {screen === 'settings' && (
        <SettingsScreen
          onLogout={handleLogout}
        />
      )}

      <View style={styles.tabBar}>
        <Tab label="Home" active={screen === 'home'} onPress={() => setScreen('home')} />
        <Tab label="Maintenance" active={screen === 'maintenance'} onPress={() => setScreen('maintenance')} />
        <Tab label="Settings" active={screen === 'settings'} onPress={() => setScreen('settings')} />
      </View>
    </SafeAreaView>
  );
}

function LoginScreen({ busy, message, onLogin }: {
  busy: boolean;
  message: string | null;
  onLogin: (email: string, password: string) => Promise<void>;
}) {
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');

  return (
    <KeyboardAvoidingView behavior={Platform.OS === 'ios' ? 'padding' : undefined} style={styles.authShell}>
      <View style={styles.authCard}>
        <Text style={styles.authTitle}>Co-Living OS</Text>
        <Text style={styles.authSub}>Sign in with the account provided by your building.</Text>
        <TextInput style={styles.input} placeholder="Email" autoCapitalize="none" keyboardType="email-address" value={email} onChangeText={setEmail} />
        <TextInput style={styles.input} placeholder="Password" secureTextEntry value={password} onChangeText={setPassword} />
        {message && <Text style={styles.error}>{message}</Text>}
        <Pressable style={styles.primaryButton} onPress={() => onLogin(email.trim(), password)} disabled={busy}>
          <Text style={styles.primaryButtonText}>{busy ? 'Signing in' : 'Sign in'}</Text>
        </Pressable>
      </View>
    </KeyboardAvoidingView>
  );
}

function ChangePasswordScreen({ busy, message, onChangePassword }: {
  busy: boolean;
  message: string | null;
  onChangePassword: (oldPassword: string, newPassword: string) => Promise<void>;
}) {
  const [oldPassword, setOldPassword] = useState('');
  const [newPassword, setNewPassword] = useState('');
  const [confirmPassword, setConfirmPassword] = useState('');
  const [localError, setLocalError] = useState<string | null>(null);

  function submit() {
    if (newPassword.length < 8) {
      setLocalError('Password must be at least 8 characters.');
      return;
    }
    if (newPassword !== confirmPassword) {
      setLocalError('Passwords do not match.');
      return;
    }
    setLocalError(null);
    void onChangePassword(oldPassword, newPassword);
  }

  return (
    <KeyboardAvoidingView behavior={Platform.OS === 'ios' ? 'padding' : undefined} style={styles.authShell}>
      <View style={styles.authCard}>
        <Text style={styles.authTitle}>Change password</Text>
        <Text style={styles.authSub}>Your first sign-in uses a temporary password.</Text>
        <TextInput style={styles.input} placeholder="Temporary password" secureTextEntry value={oldPassword} onChangeText={setOldPassword} />
        <TextInput style={styles.input} placeholder="New password" secureTextEntry value={newPassword} onChangeText={setNewPassword} />
        <TextInput style={styles.input} placeholder="Confirm new password" secureTextEntry value={confirmPassword} onChangeText={setConfirmPassword} />
        {(localError || message) && <Text style={styles.error}>{localError || message}</Text>}
        <Pressable style={styles.primaryButton} onPress={submit} disabled={busy}>
          <Text style={styles.primaryButtonText}>{busy ? 'Saving' : 'Save password'}</Text>
        </Pressable>
      </View>
    </KeyboardAvoidingView>
  );
}

function HomeScreen({ context, importantNotices, onReadNotice }: {
  context: MyApartmentContextDto | null;
  importantNotices: ResidentNotificationDto[];
  onReadNotice: (id: string) => void;
}) {
  return (
    <ScrollView style={styles.content} contentContainerStyle={styles.contentPad}>
      <View style={styles.panel}>
        <Text style={styles.panelLabel}>My apartment</Text>
        <Text style={styles.title}>{context?.buildingName || 'No building assigned'}</Text>
        <Text style={styles.muted}>
          {context ? `${context.unitNumber || context.apartmentName}${context.rooms[0]?.number ? ` · Room ${context.rooms[0].number}` : ''}` : 'Ask your building team to assign your room.'}
        </Text>
      </View>

      <View style={styles.panel}>
        <Text style={styles.panelLabel}>Important notices</Text>
        {importantNotices.length === 0 && <Text style={styles.muted}>No unread important notices.</Text>}
        {importantNotices.map((notice) => (
          <View style={styles.notice} key={notice.id}>
            <Text style={styles.noticeTitle}>{notice.title}</Text>
            <Text style={styles.noticeBody}>{notice.body}</Text>
            <Pressable onPress={() => onReadNotice(notice.id)}>
              <Text style={styles.linkText}>Mark as read</Text>
            </Pressable>
          </View>
        ))}
      </View>
    </ScrollView>
  );
}

function MaintenanceScreen({ token, context, locationOptions, items, busy, run, onChanged }: {
  token: string;
  context: MyApartmentContextDto | null;
  locationOptions: LocationOption[];
  items: MaintenanceRequestDto[];
  busy: boolean;
  run: (action: () => Promise<void>, success?: string) => Promise<void>;
  onChanged: () => Promise<void>;
}) {
  const [showForm, setShowForm] = useState(false);
  const [title, setTitle] = useState('');
  const [description, setDescription] = useState('');
  const [category, setCategory] = useState<MaintenanceCategory>('Plumbing');
  const [priority, setPriority] = useState<MaintenancePriority>('Normal');
  const [locationKey, setLocationKey] = useState('');
  const selectedLocation = locationOptions.find((option) => option.key === locationKey) || locationOptions[0];

  useEffect(() => {
    if (!locationKey && locationOptions.length > 0) {
      setLocationKey(locationOptions[0].key);
    }
  }, [locationOptions, locationKey]);

  async function submit() {
    if (!selectedLocation || !title.trim() || !description.trim()) return;
    await run(async () => {
      await api.createMaintenance(token, {
        title: title.trim(),
        description: description.trim(),
        category,
        priority,
        ...selectedLocation.payload,
      });
      setTitle('');
      setDescription('');
      setShowForm(false);
      await onChanged();
    }, 'Request sent');
  }

  return (
    <ScrollView style={styles.content} contentContainerStyle={styles.contentPad}>
      <View style={styles.panel}>
        <View style={styles.rowBetween}>
          <View>
            <Text style={styles.panelLabel}>Maintenance</Text>
            <Text style={styles.title}>Requests</Text>
          </View>
          <Pressable style={styles.secondaryButton} onPress={() => setShowForm((value) => !value)} disabled={!context}>
            <Text style={styles.secondaryButtonText}>{showForm ? 'Close' : 'New'}</Text>
          </Pressable>
        </View>

        {showForm && (
          <View style={styles.formBlock}>
            <OptionRow label="Category" value={category} values={categories} onChange={(value) => setCategory(value as MaintenanceCategory)} />
            <OptionRow label="Priority" value={priority} values={priorities} onChange={(value) => setPriority(value as MaintenancePriority)} />
            <OptionRow label="Location" value={selectedLocation?.key || ''} values={locationOptions.map((option) => option.key)} labels={Object.fromEntries(locationOptions.map((option) => [option.key, option.label]))} onChange={setLocationKey} />
            <TextInput style={styles.input} placeholder="Short title" value={title} onChangeText={setTitle} />
            <TextInput style={[styles.input, styles.textArea]} placeholder="Describe the problem" value={description} onChangeText={setDescription} multiline />
            <Pressable style={styles.primaryButton} onPress={submit} disabled={busy || !context}>
              <Text style={styles.primaryButtonText}>{busy ? 'Sending' : 'Submit request'}</Text>
            </Pressable>
          </View>
        )}
      </View>

      {items.map((item) => (
        <View style={styles.panel} key={item.id}>
          <View style={styles.rowBetween}>
            <Text style={styles.itemTitle}>{item.title}</Text>
            <Text style={styles.status}>{item.status}</Text>
          </View>
          <Text style={styles.muted}>{item.description}</Text>
          {item.assignedStaffName && <Text style={styles.muted}>Assigned to {item.assignedStaffName}</Text>}
          {item.completionNotes && <Text style={styles.noticeBody}>{item.completionNotes}</Text>}
          <MaintenanceActions token={token} item={item} run={run} onChanged={onChanged} />
        </View>
      ))}

      {items.length === 0 && (
        <View style={styles.panel}>
          <Text style={styles.muted}>No requests yet.</Text>
        </View>
      )}
    </ScrollView>
  );
}

function MaintenanceActions({ token, item, run, onChanged }: {
  token: string;
  item: MaintenanceRequestDto;
  run: (action: () => Promise<void>, success?: string) => Promise<void>;
  onChanged: () => Promise<void>;
}) {
  const canCancel = ['Reported', 'Acknowledged', 'Assigned'].includes(item.status);
  const canRate = item.status === 'Completed' && !item.residentRating;

  if (canCancel) {
    return (
      <Pressable style={styles.secondaryButton} onPress={() => run(async () => {
        await api.cancelMaintenance(token, item.id);
        await onChanged();
      }, 'Request cancelled')}>
        <Text style={styles.secondaryButtonText}>Cancel request</Text>
      </Pressable>
    );
  }

  if (canRate) {
    return (
      <View style={styles.ratingRow}>
        {[1, 2, 3, 4, 5].map((rating) => (
          <Pressable key={rating} style={styles.ratingButton} onPress={() => run(async () => {
            await api.rateMaintenance(token, item.id, rating);
            await onChanged();
          }, 'Thanks for rating')}>
            <Text style={styles.ratingText}>{rating}</Text>
          </Pressable>
        ))}
      </View>
    );
  }

  if (item.residentRating) {
    return <Text style={styles.muted}>Your rating: {item.residentRating}/5</Text>;
  }

  return null;
}

function SettingsScreen({ onLogout }: { onLogout: () => Promise<void> }) {
  function openSupport(subject: string) {
    const url = `mailto:${SUPPORT_EMAIL}?subject=${encodeURIComponent(subject)}`;
    void Linking.openURL(url);
  }

  return (
    <ScrollView style={styles.content} contentContainerStyle={styles.contentPad}>
      <View style={styles.panel}>
        <Text style={styles.panelLabel}>Support</Text>
        <Pressable style={styles.settingsRow} onPress={() => openSupport('Resident support request')}>
          <Text style={styles.settingsText}>Contact support</Text>
        </Pressable>
        <Pressable style={styles.settingsRow} onPress={() => Linking.openURL(PRIVACY_POLICY_URL)}>
          <Text style={styles.settingsText}>Privacy policy</Text>
        </Pressable>
        <Pressable style={styles.settingsRow} onPress={() => openSupport('Account deletion request')}>
          <Text style={styles.dangerText}>Request account deletion</Text>
        </Pressable>
      </View>
      <Pressable style={styles.logoutButton} onPress={onLogout}>
        <Text style={styles.logoutText}>Logout</Text>
      </Pressable>
    </ScrollView>
  );
}

function Tab({ label, active, onPress }: { label: string; active: boolean; onPress: () => void }) {
  return (
    <Pressable style={[styles.tab, active && styles.tabActive]} onPress={onPress}>
      <Text style={[styles.tabText, active && styles.tabTextActive]}>{label}</Text>
    </Pressable>
  );
}

function OptionRow({ label, value, values, labels, onChange }: {
  label: string;
  value: string;
  values: string[];
  labels?: Record<string, string>;
  onChange: (value: string) => void;
}) {
  return (
    <View style={styles.optionBlock}>
      <Text style={styles.optionLabel}>{label}</Text>
      <ScrollView horizontal showsHorizontalScrollIndicator={false}>
        {values.map((item) => (
          <Pressable key={item} style={[styles.optionPill, value === item && styles.optionPillActive]} onPress={() => onChange(item)}>
            <Text style={[styles.optionText, value === item && styles.optionTextActive]}>{labels?.[item] || item}</Text>
          </Pressable>
        ))}
      </ScrollView>
    </View>
  );
}

function buildLocationOptions(context: MyApartmentContextDto | null): LocationOption[] {
  if (!context) return [];

  const options: LocationOption[] = [];

  context.rooms.forEach((room) => {
    options.push({
      key: `room:${room.id}`,
      label: `Room ${room.number}`,
      payload: { roomId: room.id },
    });
  });

  options.push({
    key: `apartment:${context.apartmentId}`,
    label: context.unitNumber ? `Unit ${context.unitNumber}` : 'My apartment',
    payload: { apartmentId: context.apartmentId },
  });

  if (context.buildingId) {
    options.push({
      key: `building:${context.buildingId}`,
      label: 'Building common area',
      payload: { buildingId: context.buildingId },
    });
  }

  return options;
}

const styles = StyleSheet.create({
  shell: {
    flex: 1,
    backgroundColor: '#F4F6F8',
  },
  centered: {
    flex: 1,
    alignItems: 'center',
    justifyContent: 'center',
    gap: 12,
    backgroundColor: '#F4F6F8',
  },
  header: {
    paddingHorizontal: 20,
    paddingTop: 8,
    paddingBottom: 12,
    flexDirection: 'row',
    alignItems: 'center',
    justifyContent: 'space-between',
  },
  appName: {
    fontSize: 22,
    fontWeight: '800',
    color: '#17202A',
  },
  headerSub: {
    color: '#6B7785',
    marginTop: 2,
  },
  content: {
    flex: 1,
  },
  contentPad: {
    padding: 16,
    paddingBottom: 100,
    gap: 12,
  },
  panel: {
    backgroundColor: '#FFFFFF',
    borderRadius: 12,
    borderWidth: 1,
    borderColor: '#DFE5EB',
    padding: 16,
    gap: 10,
  },
  panelLabel: {
    color: '#6B7785',
    fontSize: 12,
    fontWeight: '700',
    textTransform: 'uppercase',
  },
  title: {
    color: '#17202A',
    fontSize: 22,
    fontWeight: '800',
  },
  itemTitle: {
    flex: 1,
    color: '#17202A',
    fontSize: 17,
    fontWeight: '800',
  },
  muted: {
    color: '#6B7785',
    lineHeight: 20,
  },
  notice: {
    borderTopWidth: 1,
    borderTopColor: '#EDF1F5',
    paddingTop: 10,
    gap: 6,
  },
  noticeTitle: {
    color: '#17202A',
    fontWeight: '800',
    fontSize: 16,
  },
  noticeBody: {
    color: '#465669',
    lineHeight: 20,
  },
  linkText: {
    color: '#2563EB',
    fontWeight: '800',
  },
  rowBetween: {
    flexDirection: 'row',
    alignItems: 'center',
    justifyContent: 'space-between',
    gap: 12,
  },
  status: {
    overflow: 'hidden',
    borderRadius: 999,
    backgroundColor: '#EEF4FF',
    color: '#174EA6',
    paddingHorizontal: 8,
    paddingVertical: 4,
    fontSize: 12,
    fontWeight: '800',
  },
  formBlock: {
    gap: 10,
    paddingTop: 10,
  },
  input: {
    minHeight: 46,
    borderRadius: 10,
    borderWidth: 1,
    borderColor: '#D7DEE6',
    backgroundColor: '#FFFFFF',
    paddingHorizontal: 12,
    color: '#17202A',
  },
  textArea: {
    minHeight: 110,
    textAlignVertical: 'top',
    paddingTop: 12,
  },
  primaryButton: {
    minHeight: 48,
    borderRadius: 10,
    backgroundColor: '#2563EB',
    alignItems: 'center',
    justifyContent: 'center',
    paddingHorizontal: 14,
  },
  primaryButtonText: {
    color: '#FFFFFF',
    fontWeight: '800',
  },
  secondaryButton: {
    minHeight: 40,
    borderRadius: 10,
    backgroundColor: '#EDF2F7',
    alignItems: 'center',
    justifyContent: 'center',
    paddingHorizontal: 12,
    marginTop: 10,
  },
  secondaryButtonText: {
    color: '#243142',
    fontWeight: '800',
  },
  iconButton: {
    minHeight: 36,
    borderRadius: 10,
    backgroundColor: '#EDF2F7',
    paddingHorizontal: 12,
    justifyContent: 'center',
  },
  iconButtonText: {
    color: '#243142',
    fontWeight: '800',
  },
  message: {
    marginHorizontal: 16,
    marginBottom: 8,
    color: '#174EA6',
    backgroundColor: '#EEF4FF',
    padding: 10,
    borderRadius: 10,
    overflow: 'hidden',
  },
  error: {
    color: '#BD1E1E',
    backgroundColor: '#FFF1F1',
    padding: 10,
    borderRadius: 10,
    overflow: 'hidden',
  },
  authShell: {
    flex: 1,
    backgroundColor: '#F4F6F8',
    justifyContent: 'center',
    padding: 20,
  },
  authCard: {
    backgroundColor: '#FFFFFF',
    borderRadius: 12,
    borderWidth: 1,
    borderColor: '#DFE5EB',
    padding: 18,
    gap: 12,
  },
  authTitle: {
    fontSize: 28,
    fontWeight: '900',
    color: '#17202A',
  },
  authSub: {
    color: '#6B7785',
    marginBottom: 8,
    lineHeight: 20,
  },
  tabBar: {
    position: 'absolute',
    left: 14,
    right: 14,
    bottom: 16,
    minHeight: 58,
    borderRadius: 14,
    backgroundColor: '#FFFFFF',
    borderWidth: 1,
    borderColor: '#DFE5EB',
    flexDirection: 'row',
    padding: 6,
    gap: 6,
  },
  tab: {
    flex: 1,
    alignItems: 'center',
    justifyContent: 'center',
    borderRadius: 10,
  },
  tabActive: {
    backgroundColor: '#EEF4FF',
  },
  tabText: {
    color: '#6B7785',
    fontWeight: '800',
    fontSize: 12,
  },
  tabTextActive: {
    color: '#174EA6',
  },
  optionBlock: {
    gap: 8,
  },
  optionLabel: {
    color: '#6B7785',
    fontWeight: '800',
    fontSize: 12,
  },
  optionPill: {
    borderRadius: 999,
    borderWidth: 1,
    borderColor: '#D7DEE6',
    backgroundColor: '#FFFFFF',
    paddingHorizontal: 12,
    paddingVertical: 8,
    marginRight: 8,
  },
  optionPillActive: {
    borderColor: '#2563EB',
    backgroundColor: '#EEF4FF',
  },
  optionText: {
    color: '#465669',
    fontWeight: '700',
  },
  optionTextActive: {
    color: '#174EA6',
  },
  ratingRow: {
    flexDirection: 'row',
    gap: 8,
    marginTop: 10,
  },
  ratingButton: {
    width: 40,
    height: 40,
    borderRadius: 10,
    backgroundColor: '#EEF4FF',
    alignItems: 'center',
    justifyContent: 'center',
  },
  ratingText: {
    color: '#174EA6',
    fontWeight: '900',
  },
  settingsRow: {
    minHeight: 48,
    justifyContent: 'center',
    borderTopWidth: 1,
    borderTopColor: '#EDF1F5',
  },
  settingsText: {
    color: '#17202A',
    fontWeight: '700',
  },
  dangerText: {
    color: '#BD1E1E',
    fontWeight: '800',
  },
  logoutButton: {
    minHeight: 48,
    borderRadius: 10,
    backgroundColor: '#FFF1F1',
    alignItems: 'center',
    justifyContent: 'center',
  },
  logoutText: {
    color: '#BD1E1E',
    fontWeight: '900',
  },
});
