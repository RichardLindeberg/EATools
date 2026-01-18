import React, { useEffect, useState } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { useController, useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { apiClient } from '../../api/client';
import { ApplicationServiceFormSchema, ApplicationServiceFormData } from '../../utils/formValidation';
import { useEntity } from '../../hooks/useEntity';
import { EntityFormTemplate } from '../../components/forms/EntityFormTemplate';
import { FormFieldWrapper } from '../../components/forms/FormFieldWrapper';
import { DynamicFieldArray } from '../../components/forms/DynamicFieldArray';
import { DiscardChangesModal } from '../../components/forms/DiscardChangesModal';
import { hasUnsavedChanges, deepClone } from '../../utils/formHelpers';
import { updateApplicationServiceWithCommands } from '../../utils/commandDispatcher';
import './EntityFormPage.css';

interface ApplicationServiceFormPageProps {
  isEdit?: boolean;
}

export function ApplicationServiceFormPage({ isEdit = false }: ApplicationServiceFormPageProps) {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const [showDiscardModal, setShowDiscardModal] = useState(false);

  const { data: existingService, isLoading: isLoadingService } = useEntity(
    isEdit ? 'application-services' : '',
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
  } = useForm<ApplicationServiceFormData>({
    resolver: zodResolver(ApplicationServiceFormSchema),
    defaultValues: {
      name: '',
      description: '',
      application: '',
      type: 'Synchronous',
      status: 'Available',
      owner: '',
      serviceContract: '',
      sla: 99.9,
      timeout: 30000,
      retryPolicy: '',
      businessCapabilityId: '',
      consumerAppId: '',
      tags: [],
    },
  });

  const { field: tagsField } = useController({
    control,
    name: 'tags',
  });

  const watchedValues = watch();
  const [originalValues, setOriginalValues] = useState<ApplicationServiceFormData | null>(null);
  const hasChanges = originalValues ? hasUnsavedChanges(originalValues, watchedValues) : false;

  useEffect(() => {
    if (existingService) {
      const normalized: ApplicationServiceFormData = {
        name: (existingService as any).name || '',
        description: (existingService as any).description || '',
        application: (existingService as any).application || '',
        type: (existingService as any).type || 'Synchronous',
        status: (existingService as any).status || 'Available',
        owner: (existingService as any).owner || '',
        serviceContract: (existingService as any).serviceContract || '',
        sla: (existingService as any).sla ?? 99.9,
        timeout: (existingService as any).timeout ?? 30000,
        retryPolicy: (existingService as any).retryPolicy || '',
        businessCapabilityId: (existingService as any).businessCapabilityId || '',
        consumerAppId: '',
        tags: (existingService as any).tags || [],
      };

      reset(normalized);
      setOriginalValues(deepClone(normalized));
    }
  }, [existingService, reset]);

  useEffect(() => {
    if (!isEdit) {
      const defaults: ApplicationServiceFormData = {
        name: '',
        description: '',
        application: '',
        type: 'Synchronous',
        status: 'Available',
        owner: '',
        serviceContract: '',
        sla: 99.9,
        timeout: 30000,
        retryPolicy: '',
        businessCapabilityId: '',
        consumerAppId: '',
        tags: [],
      };
      setOriginalValues(defaults);
    }
  }, [isEdit]);

  const onSubmit = async (data: ApplicationServiceFormData) => {
    try {
      let response;

      if (isEdit && id) {
        const result = await updateApplicationServiceWithCommands(
          id,
          originalValues || {},
          data
        );
        if ((result as any)?.id) {
          navigate(`/entities/application-services/${(result as any).id}`);
        }
      } else {
        response = await apiClient.post('/application-services', data);
        if (response.data?.id) {
          navigate(`/entities/application-services/${response.data.id}`);
        }
      }
    } catch (error: any) {
      const errorMessage =
        error?.response?.data?.detail || error?.message || 'Failed to save application service';

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
      navigate('/entities/application-services');
    }
  };

  const handleConfirmDiscard = () => {
    setShowDiscardModal(false);
    navigate('/entities/application-services');
  };

  if (isEdit && isLoadingService) {
    return <div className="entity-form-loading">Loading...</div>;
  }

  return (
    <>
      <EntityFormTemplate
        title={isEdit ? 'Edit Application Service' : 'Create Application Service'}
        subtitle={
          isEdit ? `Editing: ${existingService?.name || ''}` : 'Add a new application service'
        }
        isEdit={isEdit}
        isLoading={isSubmitting}
        error={null}
        onSubmit={handleReactFormSubmit(onSubmit)}
        onCancel={handleCancel}
        submitButtonLabel={isEdit ? 'Save Changes' : 'Create Service'}
      >
        <div className="entity-form-section">
          <h3 className="entity-form-section-title">Basic Information</h3>

          <div className="entity-form-section-grid">
            <FormFieldWrapper
              label="Service Name"
              required
              error={errors.name?.message}
              htmlFor="name"
            >
              <input
                id="name"
                type="text"
                placeholder="e.g., Order API"
                {...register('name')}
              />
            </FormFieldWrapper>

            <FormFieldWrapper
              label="Application"
              required
              error={errors.application?.message}
              htmlFor="application"
            >
              <input
                id="application"
                type="text"
                placeholder="Enter application ID"
                {...register('application')}
              />
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
          </div>

          <FormFieldWrapper
            label="Description"
            error={errors.description?.message}
            htmlFor="description"
          >
            <textarea
              id="description"
              placeholder="Describe the service..."
              {...register('description')}
            />
          </FormFieldWrapper>
        </div>

        <div className="entity-form-section">
          <h3 className="entity-form-section-title">Service Configuration</h3>

          <div className="entity-form-section-grid">
            <FormFieldWrapper
              label="Type"
              required
              error={errors.type?.message}
              htmlFor="type"
            >
              <select id="type" {...register('type')}>
                <option value="Synchronous">Synchronous</option>
                <option value="Asynchronous">Asynchronous</option>
                <option value="Webhook">Webhook</option>
              </select>
            </FormFieldWrapper>

            <FormFieldWrapper
              label="Status"
              required
              error={errors.status?.message}
              htmlFor="status"
            >
              <select id="status" {...register('status')}>
                <option value="Available">Available</option>
                <option value="Unavailable">Unavailable</option>
                <option value="Deprecated">Deprecated</option>
              </select>
            </FormFieldWrapper>

            <FormFieldWrapper
              label="SLA (%)"
              error={errors.sla?.message}
              htmlFor="sla"
              helpText="99.0 - 99.99"
            >
              <input
                id="sla"
                type="number"
                step="0.01"
                placeholder="99.9"
                {...register('sla', { valueAsNumber: true })}
              />
            </FormFieldWrapper>

            <FormFieldWrapper
              label="Timeout (ms)"
              error={errors.timeout?.message}
              htmlFor="timeout"
            >
              <input
                id="timeout"
                type="number"
                min="0"
                placeholder="30000"
                {...register('timeout', { valueAsNumber: true })}
              />
            </FormFieldWrapper>

            <FormFieldWrapper
              label="Retry Policy"
              error={errors.retryPolicy?.message}
              htmlFor="retryPolicy"
            >
              <input
                id="retryPolicy"
                type="text"
                placeholder="e.g., exponential-backoff"
                {...register('retryPolicy')}
              />
            </FormFieldWrapper>
          </div>

          <div className="entity-form-section-grid">
            <FormFieldWrapper
              label="Business Capability"
              error={errors.businessCapabilityId?.message}
              htmlFor="businessCapabilityId"
              helpText="Optional. Sets capability via command"
            >
              <input
                id="businessCapabilityId"
                type="text"
                placeholder="Capability ID"
                {...register('businessCapabilityId')}
              />
            </FormFieldWrapper>

            <FormFieldWrapper
              label="Add Consumer Application"
              error={errors.consumerAppId?.message}
              htmlFor="consumerAppId"
              helpText="Optional. Adds one consumer via command"
            >
              <input
                id="consumerAppId"
                type="text"
                placeholder="Consumer application ID"
                {...register('consumerAppId')}
              />
            </FormFieldWrapper>
          </div>

          <FormFieldWrapper
            label="Service Contract"
            error={errors.serviceContract?.message}
            htmlFor="serviceContract"
            helpText="OpenAPI spec, schema, or contract definition"
          >
            <textarea
              id="serviceContract"
              placeholder="Enter service contract (OpenAPI, schema, etc.)..."
              {...register('serviceContract')}
            />
          </FormFieldWrapper>

          <DynamicFieldArray
            label="Tags"
            value={tagsField.value || []}
            onChange={tagsField.onChange}
            placeholder="e.g., api, core"
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
