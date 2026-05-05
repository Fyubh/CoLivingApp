import { useState } from 'react';
import { KeyboardAvoidingView, Linking, Platform, Pressable, ScrollView, Text, View } from 'react-native';
import { useSafeAreaInsets } from 'react-native-safe-area-context';
import { Button, Field, GradientBackground, Icon, TextField } from '../components';
import { SUPPORT_EMAIL } from '../config';
import { useSession } from '../state/SessionContext';
import { T, TYPE } from '../theme/tokens';

export function LoginScreen() {
  const insets = useSafeAreaInsets();
  const { login, busy, error, clearError } = useSession();
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [showPw, setShowPw] = useState(false);

  const canSubmit = email.trim().length > 0 && password.length > 0 && !busy;

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
          <View
            style={{
              width: 56,
              height: 56,
              borderRadius: 16,
              backgroundColor: T.accent,
              alignItems: 'center',
              justifyContent: 'center',
              shadowColor: T.accent,
              shadowOpacity: 0.32,
              shadowRadius: 16,
              shadowOffset: { width: 0, height: 6 },
              elevation: 6,
            }}
          >
            <Icon name="logo-key" size={28} color="#fff" />
          </View>
          <Text style={[TYPE.title, { color: T.text, marginTop: 16 }]}>Войти в Co-Living</Text>
          <Text style={[TYPE.body, { color: T.text2, marginTop: 6 }]}>
            Email и временный пароль выдаёт администратор.
          </Text>

          <View style={{ marginTop: 28, gap: 14 }}>
            <Field label="Email">
              <TextField
                value={email}
                onChangeText={(v) => {
                  clearError();
                  setEmail(v);
                }}
                placeholder="you@coliving.app"
                keyboardType="email-address"
                leading={<Icon name="mail" size={18} color={T.text2} />}
              />
            </Field>
            <Field label="Пароль">
              <TextField
                value={password}
                onChangeText={(v) => {
                  clearError();
                  setPassword(v);
                }}
                placeholder="••••••••"
                secureTextEntry={!showPw}
                leading={<Icon name="lock" size={18} color={T.text2} />}
                trailing={
                  <Pressable onPress={() => setShowPw((s) => !s)}>
                    <Icon name={showPw ? 'eye-off' : 'eye'} size={18} color={T.text2} />
                  </Pressable>
                }
                error={error}
              />
            </Field>
          </View>

          <View style={{ marginTop: 20 }}>
            <Button
              kind="primary"
              size="lg"
              full
              disabled={!canSubmit}
              onPress={() => void login(email.trim(), password)}
            >
              {busy ? 'Входим…' : 'Войти'}
            </Button>
          </View>

          <View style={{ flex: 1 }} />

          <Pressable
            onPress={() =>
              Linking.openURL(`mailto:${SUPPORT_EMAIL}?subject=${encodeURIComponent('Проблема со входом')}`)
            }
            style={{ marginTop: 24 }}
          >
            <Text style={[TYPE.footnote, { color: T.text2, textAlign: 'center' }]}>
              Проблемы со входом?{' '}
              <Text style={{ color: T.accent, fontWeight: '700' }}>Поддержка</Text>
            </Text>
          </Pressable>
        </ScrollView>
      </KeyboardAvoidingView>
    </GradientBackground>
  );
}
