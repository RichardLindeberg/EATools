import React, { useEffect, useState } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { useController, useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { apiClient } from '../../api/client';
import { ServerFormSchema, ServerFormData } from '../../utils/formValidation';
import { useEntity } from '../../hooks/useEntity';
import { EntityFormTemplate } from '../../components/forms/EntityFormTemplate';
import { FormFieldWrapper } from '../../components/forms/FormFieldWrapper';
import { DynamicFieldArray } from '../../components/forms/DynamicFieldArray';
import { DiscardChangesModal } from '../../components/forms/DiscardChangesModal';
import { hasUnsavedChanges, deepClone } from '../../utils/formHelpers';
import './EntityFormPage.css';

interface ServerFormPageProps {
  isEdit?: boolean;
}

export function ServerFormPage({ isEdit = false }: ServerFormPageProps) {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const [showDiscardModal, setShowDiscardModal] = useState(false);

  const { data: existingServer, isLoading: isLoadingServer } = useEntity(
    isEdit ? 'servers' : '',
    isEdit ? id : ''
  );

  const {
    register,
    control,
    watch,
    handleSubmit: handleReactFormSubmit,
    formState: { errors, isSubmitting },
    setError,
    reset,
  } = useForm<ServerFormData>({
    resolver: zodResolver(ServerFormSchema),
    defaultValues: isEdit
      ? existingServer || {}
      : {
          name: '',
          host: '',
          ipAddress: '',
          environment: 'Production',
          osType: 'Linux',
          osVersion: '',
          owner: '',
          backupSchedule: 'Never',
          description: '',
          tags: [],
        },
  });

  const { field: tagsField } = useController({
    control,
    name: 'tags',
  });

  const watchedValues = watch();
  const [originalValues] = useState(() => deepClone(watchedValues));
  const hasChanges = hasUnsavedChanges(originalValues, watchedValues);

  useEffect(() => {
    if (existingServer) {
      reset(existingServer);
    }
  }, [existingServer, reset]);

  const onSubmit = async (data: ServerFormData) => {
    try {
      let response;

      if (isEdit && id) {
        response = await apiClient.patch(`/servers/${id}`, data);
      } else {
        response = await apiClient.post('/servers', data);
      }

      if (response.data?.id) {
        navigate(`/entities/servers/${response.data.id}`);
      }
    } catch (error: any) {
      const errorMessage =
        error?.response?.data?.detail || error?.message || 'Failed to save server';

      if (error?.response?.status === 422 && error?.response?.data?.errors) {
        error.response.data.errors.forEach((err: any) => {
          if (err.field) {
            setError(err.field as any, {
              type: 'server',
              message: err.message,
            });
          }
        });
      }
    }
  };

  const handleCancel = () => {
    if (hasChanges) {
      setShowDiscardModal(true);
    } else {
      navigate('/entities/servers');
    }
  };

  const handleConfirmDiscard = () => {
    setShowDiscardModal(false);
    navigate('/entities/servers');
  };

  if (isEdit && isLoadingServer) {
    return <div className="entity-form-loading">Loading...</div>;
  }

  return (
    <>
      <EntityFormTemplate
        title={isEdit ? 'Edit Server' : 'Create Server'}
        subtitle={isEdit ? `Editing: ${existingServer?.name || ''}` : 'Add a new server'}
        isEdit={isEdit}
        isLoading={isSubmitting}
        error={null}
        onSubmit={handleReactFormSubmit(onSubmit)}
        onCancel={handleCancel}
        submitButtonLabel={isEdit ? 'Save Changes' : 'Create Server'}
      >
        <div className="entity-form-section">
          <h3 className="entity-form-section-title">Basic Information</h3>

          <div className="entity-form-section-grid">
            <FormFieldWrapper
              label="Server Name"
              required
              error={errors.name?.message}
              htmlFor="name"
            >
              <input
                id="name"
                type="text"
                placeholder="e.g., web-server-01"
                {...register('name')}
              />
            </FormFieldWrapper>

            <FormFieldWrapper
              label="Host"
              required
              error={errors.host?.message}
              htmlFor="host"
            >
              <input
                id="host"
                type="text"
                placeholder="e.g., prod-web-01.example.com"
                {...register('host')}
              />
            </FormFieldWrapper>

            <FormFieldWrapper
              label="IP Address"
              error={errors.ipAddress?.message}
              htmlFor="ipAddress"
              helpText="IPv4 or IPv6"
            >
              <input
                id="ipAddress"
                type="text"
                placeholder="e.g., 192.168.1.100"
                {...register('ipAddress')}
              />
            </FormFieldWrapper>
          </div>

          <FormFieldWrapper
            label="Description"
            error={errors.description?.message}
            htmlFor="description"
          >
            <textarea
              id="description"
              placeholder="Describe the server's purpose..."
              {...register('description')}
            />
          </FormFieldWrapper>
        </div>

        <div className="entity-form-section">
          <h3 className="entity-form-section-title">Configuration</h3>

          <div className="entity-form-section-grid">
            <FormFieldWrapper
              label="OS Type"
              required
              error={errors.osType?.message}
              htmlFor="osType"
            >
              <select id="osType" {...register('osType')}>
                <option value="Linux">Linux</option>
                <option value="Windows">Windows</option>
                <option value="macOS">macOS</option>
                <option value="Cloud">Cloud</option>
              </select>
            </FormFieldWrapper>

            <FormFieldWrapper
              label="OS Version"
              error={errors.osVersion?.message}
              htmlFor="osVersion"
            >
              <input
                id="osVersion"
                type="text"
                placeholder="e.g., Ubuntu 20.04"
                {...register('osVersion')}
              />
            </FormFieldWrapper>

            <FormFieldWrapper
              label="Environment"
              required
              error={errors.environment?.message}
              htmlFor="environment"
            >
              <select id="environment" {...register('environment')}>
                <option value="Production">Production</option>
                <option value="Staging">Staging</option>
                <option value="Development">Development</option>
                <option value="Test">Test</option>
              </select>
            </FormFieldWrapper>

            <FormFieldWrapper
              label="Owner"
              required
              error={errors.owner?.message}
              htmlFor="owner"
            >
              <input
                id="owner"
                type="text"
                placeholder="Enter owner user ID"
                {...register('owner')}
              />
            </FormFieldWrapper>

            <FormFieldWrapper
              label="Backup Schedule"
              error={errors.backupSchedule?.message}
              htmlFor="backupSchedule"
            >
              <select id="backupSchedule" {...register('backupSchedule')}>
                <option value="Never">Never</option>
                <option value="Daily">Daily</option>
                <option value="Weekly">Weekly</option>
                <option value="Monthly">Monthly</option>
              </select>
            </FormFieldWrapper>
          </div>

          <DynamicFieldArray
            label="Tags"
            value={tagsField.value || []}
            onChange={tagsField.onChange}
            placeholder="e.g., production, web-tier"
            error={errors.tags?.message}
          />
        </div>
      </EntityFormTemplate>

      <DiscardChangesModal
        isOpen={showDiscardModal}
        onConfirm={handleConfirmDiscard}
        onCancel={() => setShowDiscardModal(false)}
      />
    </>
  );
}
