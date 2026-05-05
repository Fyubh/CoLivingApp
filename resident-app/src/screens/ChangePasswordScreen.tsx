import { useState } from 'react';
import { KeyboardAvoidingView, Platform, Pressable, ScrollView, Text, View } from 'react-native';
import { useSafeAreaInsets } from 'react-native-safe-area-context';
import { Button, Field, Glass, GradientBackground, Icon, TextField } from '../components';
import { useSession } from '../state/SessionContext';
import { T, TYPE } from '../theme/tokens';

function strengthHint(pw: string): string | undefined {
  if (!pw) return undefined;
  if (pw.length < 8) return 'Сила: коротко (мин. 8 символов)';
  const hasDigit = /\d/.test(pw);
  const hasLetter = /[A-Za-zА-Яа-яЁё]/.test(pw);
  if (!hasDigit || !hasLetter) return 'Сила: добавьте цифру и букву';
  if (pw.length < 12) return 'Сила: хороший';
  return 'Сила: отличный';
}

export function ChangePasswordScreen() {
  const insets = useSafeAreaInsets();
  const { changePassword, busy, error, clearError } = useSession();
  const [oldPassword, setOldPassword] = useState('');
  const [newPassword, setNewPassword] = useState('');
  const [confirmPassword, setConfirmPassword] = useState('');
  const [showOld, setShowOld] = useState(false);
  const [showNew, setShowNew] = useState(false);
  const [localError, setLocalError] = useState<string | null>(null);

  function submit() {
    setLocalError(null);
    if (newPassword.length < 8) {
      setLocalError('Минимум 8 символов.');
      return;
    }
    if (!/\d/.test(newPassword) || !/[A-Za-zА-Яа-яЁё]/.test(newPassword)) {
      setLocalError('Нужна хотя бы одна цифра и одна буква.');
      return;
    }
    if (newPassword !== confirmPassword) {
      setLocalError('Пароли не совпадают.');
      return;
    }
    void changePassword(oldPassword, newPassword);
  }

  const canSubmit =
    !busy && oldPassword.length > 0 && newPassword.length >= 8 && confirmPassword.length > 0;

  return (
    <GradientBackground>
      <KeyboardAvoidingView
        behavior={Platform.OS === 'ios' ? 'padding' : 'height'}
        style={{ flex: 1 }}
      >
        <ScrollView
          contentContainerStyle={{
            flexGrow: 1,
            paddingTop: insets.top + 60,
            paddingHorizontal: 24,
            paddingBottom: insets.bottom + 24,
          }}
          keyboardShouldPersistTaps="handled"
        >
          <Glass padding={14} radius={16} style={{ marginBottom: 20 }}>
            <View style={{ flexDirection: 'row', alignItems: 'center', gap: 10 }}>
              <View
                style={{
                  width: 28,
                  height: 28,
                  borderRadius: 14,
                  backgroundColor: T.warningSoft,
                  alignItems: 'center',
                  justifyContent: 'center',
                }}
              >
                <Icon name="info" size={16} color={T.warning} />
              </View>
              <Text style={[TYPE.footnote, { color: T.text2, flex: 1 }]}>
                Первый вход. Придумайте новый пароль.
              </Text>
            </View>
          </Glass>

          <Text style={[TYPE.display, { color: T.text }]}>Новый пароль</Text>
          <Text style={[TYPE.body, { color: T.text2, marginTop: 6, marginBottom: 24 }]}>
            Минимум 8 символов, цифра и буква.
          </Text>

          <View style={{ gap: 14 }}>
            <Field label="Временный пароль">
              <TextField
                value={oldPassword}
                onChangeText={(v) => {
                  clearError();
                  setLocalError(null);
                  setOldPassword(v);
                }}
                secureTextEntry={!showOld}
                trailing={
                  <Pressable onPress={() => setShowOld((s) => !s)}>
                    <Icon name={showOld ? 'eye-off' : 'eye'} size={18} color={T.text2} />
                  </Pressable>
                }
              />
            </Field>
            <Field label="Новый пароль" hint={strengthHint(newPassword)}>
              <TextField
                value={newPassword}
                onChangeText={(v) => {
                  clearError();
                  setLocalError(null);
                  setNewPassword(v);
                }}
                secureTextEntry={!showNew}
                trailing={
                  <Pressable onPress={() => setShowNew((s) => !s)}>
                    <Icon name={showNew ? 'eye-off' : 'eye'} size={18} color={T.text2} />
                  </Pressable>
                }
              />
            </Field>
            <Field label="Подтвердить">
              <TextField
                value={confirmPassword}
                onChangeText={(v) => {
                  clearError();
                  setLocalError(null);
                  setConfirmPassword(v);
                }}
                secureTextEntry
                error={localError ?? error}
              />
            </Field>
          </View>

          <View style={{ flex: 1 }} />

          <Button
            kind="primary"
            size="lg"
            full
            disabled={!canSubmit}
            onPress={submit}
            style={{ marginTop: 24 }}
          >
            {busy ? 'Сохраняем…' : 'Сохранить пароль'}
          </Button>
        </ScrollView>
      </KeyboardAvoidingView>
    </GradientBackground>
  );
}
